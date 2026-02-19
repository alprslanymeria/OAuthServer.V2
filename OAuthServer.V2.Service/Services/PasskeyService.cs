using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.Passkeys;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Repositories;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Core.UnitOfWork;

namespace OAuthServer.V2.Service.Services;

public class PasskeyService(

    IFido2 fido2,
    UserManager<User> userManager,
    ITokenService tokenService,
    IPasskeyCredentialRepository passkeyCredentialRepository,
    IGenericRepository<UserRefreshToken> userRefreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    IMapper mapper

    ) : IPasskeyService
{
    private const string AttestationCachePrefix = "fido2:attestation:";
    private const string AssertionCachePrefix = "fido2:assertion:";
    private static readonly TimeSpan ChallengeExpiration = TimeSpan.FromMinutes(5);

    private readonly IFido2 _fido2 = fido2;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasskeyCredentialRepository _passkeyCredentialRepository = passkeyCredentialRepository;
    private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository = userRefreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IMapper _mapper = mapper;

    public async Task<ServiceResult<PasskeyOptionsResponse>> RegisterBeginAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        // FIND USER
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsActive) throw new ForbiddenException("Account is deactivated.");

        // GET EXISTING CREDENTIALS FOR THIS USER
        var credentials = await _passkeyCredentialRepository.GetByUserIdAsync(userId);
        var existingCredentials = _mapper.Map<List<PublicKeyCredentialDescriptor>>(credentials);

        // MAP USER TO FIDO2 USER
        var fido2User = _mapper.Map<Fido2User>(user);

        // CREATE CREDENTIAL OPTIONS
        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fido2User,
            ExcludeCredentials = existingCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Required
            },
            AttestationPreference = AttestationConveyancePreference.None,
            Extensions = new AuthenticationExtensionsClientInputs
            {
                CredProps = true
            }
        });

        // STORE OPTIONS IN CACHE WITH UNIQUE REQUEST ID
        var requestId = Guid.NewGuid().ToString("N");

        await _cacheService.SetStringAsync($"{AttestationCachePrefix}{requestId}", options.ToJson(), ChallengeExpiration);

        return ServiceResult<PasskeyOptionsResponse>.Success(new PasskeyOptionsResponse(requestId, options));
    }

    public async Task<ServiceResult> RegisterCompleteAsync(string userId, PasskeyRegisterCompleteRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(request);

        // RETRIEVE AND INVALIDATE CACHED OPTIONS
        var optionsJson = await _cacheService.GetStringAsync($"{AttestationCachePrefix}{request.RequestId}")
            ?? throw new BusinessException("Invalid or expired passkey registration request.");

        await _cacheService.RemoveAsync($"{AttestationCachePrefix}{request.RequestId}");

        var options = CredentialCreateOptions.FromJson(optionsJson);

        // DESERIALIZE ATTESTATION RESPONSE
        var attestationResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(
            request.AttestationResponse.GetRawText())
            ?? throw new BusinessException("Invalid attestation response.");

        // VERIFY CREDENTIAL UNIQUENESS CALLBACK
        IsCredentialIdUniqueToUserAsyncDelegate uniqueCallback = async (args, cancellationToken) =>
            !await _passkeyCredentialRepository.ExistsByCredentialIdAsync(args.CredentialId, cancellationToken);

        // VERIFY AND CREATE CREDENTIAL
        var credential = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = uniqueCallback
        });

        // MAP TO ENTITY AND SET EXTERNAL PROPERTIES
        var entity = _mapper.Map<UserPasskeyCredential>(credential);
        entity.Id = Guid.NewGuid();
        entity.UserId = userId;
        entity.CreatedAt = DateTimeOffset.UtcNow;

        await _passkeyCredentialRepository.AddAsync(entity);
        await _unitOfWork.CommitAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<PasskeyOptionsResponse>> LoginBeginAsync(PasskeyLoginBeginRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<PublicKeyCredentialDescriptor> existingCredentials = [];

        // IF EMAIL PROVIDED, FIND USER AND THEIR CREDENTIALS
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new NotFoundException("User not found.");

            if (!user.IsActive) throw new ForbiddenException("Account is deactivated.");

            var credentials = await _passkeyCredentialRepository.GetByUserIdAsync(user.Id);

            if (credentials.Count == 0)
                throw new NotFoundException("No passkey credentials found for this user.");

            existingCredentials = _mapper.Map<List<PublicKeyCredentialDescriptor>>(credentials);
        }

        // CREATE ASSERTION OPTIONS
        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = existingCredentials,
            UserVerification = UserVerificationRequirement.Required,
            Extensions = new AuthenticationExtensionsClientInputs
            {
                UserVerificationMethod = true
            }
        });

        // STORE OPTIONS IN CACHE
        var requestId = Guid.NewGuid().ToString("N");

        await _cacheService.SetStringAsync( $"{AssertionCachePrefix}{requestId}", options.ToJson(), ChallengeExpiration);

        return ServiceResult<PasskeyOptionsResponse>.Success(new PasskeyOptionsResponse(requestId, options));
    }

    public async Task<ServiceResult<TokenResponse>> LoginCompleteAsync(PasskeyLoginCompleteRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // RETRIEVE AND INVALIDATE CACHED OPTIONS
        var optionsJson = await _cacheService.GetStringAsync($"{AssertionCachePrefix}{request.RequestId}")
            ?? throw new BusinessException("Invalid or expired passkey login request.");

        await _cacheService.RemoveAsync($"{AssertionCachePrefix}{request.RequestId}");

        var options = AssertionOptions.FromJson(optionsJson);

        // DESERIALIZE ASSERTION RESPONSE
        var assertionResponse = JsonSerializer.Deserialize<AuthenticatorAssertionRawResponse>(
            request.AssertionResponse.GetRawText())
            ?? throw new BusinessException("Invalid assertion response.");

        // FIND STORED CREDENTIAL BY CREDENTIAL ID
        var storedCredential = await _passkeyCredentialRepository.GetByCredentialIdAsync(assertionResponse.RawId)
            ?? throw new UnauthorizedException("Unknown passkey credential.");

        // VERIFY USER HANDLE OWNERSHIP CALLBACK
        IsUserHandleOwnerOfCredentialIdAsync ownershipCallback = async (args, cancellationToken) =>
        {
            var creds = await _passkeyCredentialRepository.GetByUserHandleAsync(args.UserHandle, cancellationToken);
            return creds.Exists(c => c.CredentialId.SequenceEqual(args.CredentialId));
        };

        // VERIFY ASSERTION
        var result = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertionResponse,
            OriginalOptions = options,
            StoredPublicKey = storedCredential.PublicKey,
            StoredSignatureCounter = storedCredential.SignCount,
            IsUserHandleOwnerOfCredentialIdCallback = ownershipCallback
        });

        // UPDATE SIGN COUNT
        storedCredential.SignCount = result.SignCount;
        storedCredential.IsBackedUp = result.IsBackedUp;
        _passkeyCredentialRepository.Update(storedCredential);

        // FIND USER AND VALIDATE
        var user = await _userManager.FindByIdAsync(storedCredential.UserId)
            ?? throw new NotFoundException("User not found.");

        if (!user.IsActive) throw new ForbiddenException("Account is deactivated.");

        // CREATE TOKEN AND SAVE REFRESH TOKEN
        var token = _tokenService.CreateToken(user);
        await SaveOrUpdateRefreshTokenAsync(user.Id, token);

        return ServiceResult<TokenResponse>.Success(token);
    }

    #region HELPERS

    private async Task SaveOrUpdateRefreshTokenAsync(string userId, TokenResponse token)
    {
        var userRefreshToken = await _userRefreshTokenRepository.FindAsync(x => x.UserId == userId);

        if (userRefreshToken is null)
        {
            await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = userId,
                Code = token.RefreshToken,
                Expiration = token.RefreshTokenExpiration
            });
        }
        else
        {
            userRefreshToken.Code = token.RefreshToken;
            userRefreshToken.Expiration = token.RefreshTokenExpiration;
            _userRefreshTokenRepository.Update(userRefreshToken);
        }

        await _unitOfWork.CommitAsync();
    }

    #endregion
}

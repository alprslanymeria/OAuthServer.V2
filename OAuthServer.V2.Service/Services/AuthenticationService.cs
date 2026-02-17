using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Repositories;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Core.UnitOfWork;

namespace OAuthServer.V2.Service.Services;

public class AuthenticationService(

    UserManager<User> userManager,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IOptions<List<Client>> optionsClient,
    IGenericRepository<UserRefreshToken> userRefreshTokenRepository) : IAuthenticationService
{
    private readonly List<Client> _clients = optionsClient.Value;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository = userRefreshTokenRepository;

    public async Task<ServiceResult<TokenResponse>> CreateTokenAsync(SignInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // FIND USER BY IDENTIFIER
        var user = request switch
        {
            { Email: not null } => await _userManager.FindByEmailAsync(request.Email),
            { UserName: not null } => await _userManager.FindByNameAsync(request.UserName),
            { PhoneNumber: not null } => await _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).SingleOrDefaultAsync(),
            _ => throw new BusinessException("Please provide either email, username, or phone number.")
        };

        if (user is null) throw new UnauthorizedException("Invalid credentials.");
        if (!user.IsActive) throw new ForbiddenException("Account is deactivated. Please contact support.");
        if (!user.EmailConfirmed && !user.PhoneNumberConfirmed) throw new ForbiddenException("Account is not verified. Please verify your email or phone number.");

        // VALIDATE PASSWORD
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid) throw new UnauthorizedException("Invalid credentials.");

        // CREATE TOKEN AND SAVE REFRESH TOKEN
        var token = _tokenService.CreateToken(user);
        await SaveOrUpdateRefreshTokenAsync(user.Id, token);

        return ServiceResult<TokenResponse>.Success(token);
    }

    public async Task<ServiceResult<TokenResponse>> CreateTokenByRefreshToken(string refreshToken)
    {
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync()
            ?? throw new NotFoundException("Refresh token not found.");

        var user = await _userManager.FindByIdAsync(existRefreshToken.UserId)
            ?? throw new NotFoundException("User not found.");

        // CREATE TOKEN
        var token = _tokenService.CreateToken(user);

        // UPDATE REFRESH TOKEN
        existRefreshToken.Code = token.RefreshToken;
        existRefreshToken.Expiration = token.RefreshTokenExpiration;

        // SINCE THE DATA RETURNED WITH THE WHERE CONDITION WAS MARKED AS "NO TRACKING" I CALLED IT USING THE UPDATE METHOD TO ENABLE TRACKING.
        // OTHERWISE, THE CHANGES WON'T BE REFLECTED IN THE DATABASE WHEN CALLING COMMIT ASYNC.
        _userRefreshTokenRepository.Update(existRefreshToken);
        await _unitOfWork.CommitAsync();

        return ServiceResult<TokenResponse>.Success(token);
    }

    public async Task<ServiceResult> RevokeRefreshToken(string refreshToken)
    {
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync()
            ?? throw new NotFoundException("Refresh token not found.");

        _userRefreshTokenRepository.Delete(existRefreshToken);
        await _unitOfWork.CommitAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<ClientTokenResponse>> CreateTokenByClient(ClientSignInRequest request)
    {
        var client = _clients.SingleOrDefault(x => x.Id == request.ClientId && x.Secret == request.ClientSecret)
            ?? throw new NotFoundException("Client not found.");

        var token = _tokenService.CreateTokenByClient(client);

        return ServiceResult<ClientTokenResponse>.Success(token);
    }

    public async Task<ServiceResult<TokenResponse>> CreateTokenByExternalLogin(string email, string? name, string googleSubjectId, string? picture)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new User
            {
                UserName = name ?? email.Split('@')[0],
                FirstName = name ?? "Google User",
                BirthDate = DateTime.UtcNow,
                Email = email,
                EmailConfirmed = true,
                Image = picture
            };

            // CREATE RANDOM PASSWORD FOR EXTERNAL LOGIN USER
            var password = Guid.NewGuid().ToString("N") + "Ax1!";
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new BusinessException(createResult.Errors.Select(e => e.Description).ToList());
            }

            // ADD GOOGLE LOGIN PROVIDER
            var loginInfo = new UserLoginInfo("Google", googleSubjectId, "Google");
            await _userManager.AddLoginAsync(user, loginInfo);
        }

        // CREATE TOKEN AND SAVE REFRESH TOKEN
        var token = _tokenService.CreateToken(user);
        await SaveOrUpdateRefreshTokenAsync(user.Id, token);

        return ServiceResult<TokenResponse>.Success(token);
    }

    #region HELPERS

    private async Task SaveOrUpdateRefreshTokenAsync(string userId, TokenResponse token)
    {
        var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == userId).SingleOrDefaultAsync();

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
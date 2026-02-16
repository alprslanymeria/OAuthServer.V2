using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Repositories;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Core.UnitOfWork;
using System.Net;

namespace OAuthServer.V2.Service.Services;

public class AuthenticationService(

    //IOptions<List<Client>> optionsClient,
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
        // CHECK SIGNIN DTO
        ArgumentNullException.ThrowIfNull(request);

        User? user = null;
         
        // GET USER BY SWITCH CASE BASED ON EMAIL OR USERNAME OR PHONENUMBER
        switch(request)
        {
            case { Email: not null }:
                user = await _userManager.FindByEmailAsync(request.Email);
                break;
            case { UserName: not null }:
                user = await _userManager.FindByNameAsync(request.UserName);
                break;
            case { PhoneNumber: not null }:
                user = await _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).SingleOrDefaultAsync();
                break;
            default:
                return ServiceResult<TokenResponse>.Fail("Invalid sign-in request. Please provide either email, username, or phone number.", HttpStatusCode.BadRequest);
        }

        // CHEK USER
        if (user is null)
        {
            return ServiceResult<TokenResponse>.Fail("Invalid credentials");
        }

        // GET PASSWORD
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            return ServiceResult<TokenResponse>.Fail("Invalid credentials");
        }

        // CREATE TOKEN
        var token = _tokenService.CreateToken(user);

        // CHECK REFRESH TOKEN
        var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

        if (userRefreshToken is null)
        {
            await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
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

        return ServiceResult<TokenResponse>.Success(token);

    }

    public async Task<ServiceResult<TokenResponse>> CreateTokenByRefreshToken(string refreshToken)
    {
        // CHECK REFRESH TOKEN
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

        if (existRefreshToken is null)
        {
            return ServiceResult<TokenResponse>.Fail("Refresh token not found", HttpStatusCode.NotFound);
        }

        // CHECK USER
        var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);

        if (user is null)
        {
            return ServiceResult<TokenResponse>.Fail("User Id not found", HttpStatusCode.NotFound);
        }

        // CREATE TOKEN
        var token = _tokenService.CreateToken(user);

        // UPDATE REFRESH TOKEN
        existRefreshToken.Code = token.RefreshToken;
        existRefreshToken.Expiration = token.RefreshTokenExpiration;

        // SINCE THE DATA RETURNED WITH THE WHERE CONDITION WAS MARKED AS "NO TRACKING" I CALLED IT USING THE UPDATE METHOD TO ENABLE TRACKING.
        // OTHERWISE, THE CHANGES WON'T BE REFLECTED IN THE DATABASE WHEN CALLING COMMIT ASYNC.
        _userRefreshTokenRepository.Update(existRefreshToken);

        // UPDATE DATABASE
        await _unitOfWork.CommitAsync();

        return ServiceResult<TokenResponse>.Success(token);
    }

    public async Task<ServiceResult> RevokeRefreshToken(string refreshToken)
    {
        var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

        if (existRefreshToken is null)
        {
            return ServiceResult.Fail("Refresh token not found", HttpStatusCode.NotFound);
        }

        _userRefreshTokenRepository.Delete(existRefreshToken);

        await _unitOfWork.CommitAsync();

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<ClientTokenResponse>> CreateTokenByClient(ClientSignInRequest request)
    {
        var client = _clients.SingleOrDefault(x => x.Id == request.ClientId && x.Secret == request.ClientSecret);

        if (client is null)
        {
            return ServiceResult<ClientTokenResponse>.Fail("ClientId or ClientSecret not found", HttpStatusCode.NotFound);
        }

        var token = _tokenService.CreateTokenByClient(client);

        return ServiceResult<ClientTokenResponse>.Success(token);
    }

    public async Task<ServiceResult<TokenResponse>> CreateTokenByExternalLogin(string email, string? name, string googleSubjectId, string? picture)
    {
        // FIND USER BY EMAIL
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            // CREATE USER IF NOT FOUND
            user = new User
            {
                UserName = name ?? email.Split('@')[0],
                Email = email,
                EmailConfirmed = true,
                Image = picture
            };

            // CREATE RANDOM PASSWORD FOR EXTERNAL LOGIN USER
            var password = Guid.NewGuid().ToString("N") + "Ax1!";
            var createResult = await _userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return ServiceResult<TokenResponse>.Fail(errors, HttpStatusCode.BadRequest);
            }

            // ADD GOOGLE LOGIN PROVIDER
            var loginInfo = new UserLoginInfo("Google", googleSubjectId, "Google");
            await _userManager.AddLoginAsync(user, loginInfo);
        }

        // CREATE TOKEN
        var token = _tokenService.CreateToken(user);

        //  SAVE REFRESH TOKEN
        var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

        if (userRefreshToken is null)
        {
            await _userRefreshTokenRepository.AddAsync(new UserRefreshToken
            {
                UserId = user.Id,
                Code = token.RefreshToken,
                Expiration = token.RefreshTokenExpiration
            });
        }
        else
        {
            userRefreshToken.Code = token.RefreshToken;
            userRefreshToken.Expiration = token.RefreshTokenExpiration;
        }

        await _unitOfWork.CommitAsync();

        return ServiceResult<TokenResponse>.Success(token);
    }
}
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace OAuthServer.V2.Service.Services;

// THIS TOKEN SERVICE MUST REMAIN "INTERNAL" BECAUSE WE'LL ONLY USE IT WITHIN THIS ASSEMBLY.
// HOWEVER, WE'LL MAKE IT "PUBLIC" FOR NOW BECAUSE WE NEED TO REGISTER IT IN THE DI CONTAINER IN THE API LAYER.
// THE BEST PRACTICE FOR THIS WOULD BE TO ADD A DI CONTAINER FRAMEWORK TO THIS ASSEMBLY AND INITIALIZE IT HERE.
public class TokenService(

    IOptions<TokenOption> options) : ITokenService
{

    private readonly TokenOption _tokenOption = options.Value;

    #region UTILS

    // PRIVATE METHOD FOR CREATIN REFREH TOKEN
    private static string CreateRefreshToken()
    {
        var numberByte = new Byte[32];
        using var rnd = RandomNumberGenerator.Create();

        rnd.GetBytes(numberByte);

        return Convert.ToBase64String(numberByte);
    }

    // PRIVATE METHOD FOR CREATE CLAIMS
    private static List<Claim> GetClaims(User user, List<string> audiences)
    {
        var claimList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claimList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        return claimList;
    }

    // PRIVATE METHOD FOR CREATE CLAIMS FOR CLIENT
    private static List<Claim> GetClaimsForClient(Client client)
    {
        var claimList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, client.Id!.ToString())
        };

        claimList.AddRange(client.Audiences!.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

        return claimList;
    }
    #endregion


    public TokenResponse CreateToken(User user)
    {
        // GET OPTION VALUES
        var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
        var refreshTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.RefreshTokenExpiration);
        var issuer = _tokenOption.Issuer;


        // JWT REQUIRES A "SigningCredentials" OBJECT. AND THIS OBJECT REQUIRES A "SecurityKey" OBJECT.
        var securityKey = SignService.GetSymetricKey(_tokenOption.SecurityKey);
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        // GET CLAIMS
        var claims = GetClaims(user, _tokenOption.Audience);

        // CREATE JWT TOKEN
        JwtSecurityToken jwtSecurityToken = new(

            issuer: issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: claims,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(jwtSecurityToken);

        var tokenDto = new TokenResponse(token, accessTokenExpiration, CreateRefreshToken(), refreshTokenExpiration);

        return tokenDto;
    }

    public ClientTokenResponse CreateTokenByClient(Client client)
    {
        // GET OPTION VALUES
        var accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOption.AccessTokenExpiration);
        var issuer = _tokenOption.Issuer;


        // JWT REQUIRES A "SigningCredentials" OBJECT. AND THIS OBJECT REQUIRES A "SecurityKey" OBJECT.
        var securityKey = SignService.GetSymetricKey(_tokenOption.SecurityKey);
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256Signature);

        // GET CLAIMS
        var claims = GetClaimsForClient(client);

        // CREATE JWT TOKEN
        JwtSecurityToken jwtSecurityToken = new(

            issuer: issuer,
            expires: accessTokenExpiration,
            notBefore: DateTime.Now,
            claims: claims,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();

        var token = handler.WriteToken(jwtSecurityToken);

        var clientTokenResponse = new ClientTokenResponse(

            AccessToken: token,
            AccessTokenExpiration: accessTokenExpiration
        );

        return clientTokenResponse;
    }
}
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Data;
using OAuthServer.V2.Service.Services;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace OAuthServer.V2.API.Extensions;

public static class CustomTokenAuth
{

    public static IServiceCollection AddCustomTokenAuth(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddIdentity<User, IdentityRole>(opt =>
        {
            opt.User.RequireUniqueEmail = true;

        }).AddIdentityEntityFrameworkStores().AddDefaultTokenProviders();

        // WHEN A TOKEN COMES IN, WE WRITE THIS CODE TO VALIDATE IT.
        // SCHEME --> SPECIFIES DIFFERENT MEMBERSHIP SYSTEMS IN THE APPLICATION. FOR EXAMPLE, ONE FOR DEALERS AND ANOTHER FOR USERS
        // WITH ADD AUTHENTICATION, WE SPECIFY THAT THIS APPLICATION WILL HAVE AN IDENTITY AUTHENTICATION.
        // WITH ADD JWT BEARER, THE MECHANISM FOR VALIDATING THE TOKEN IN THE REQUEST IS ADDED.

        // ADD AUTHENTICATION --> THE MAIN GUARD OF THE KINGDOM
        // SCHEMES --> THE GUARDS WORKING UNDER THE MAIN GUARD. BEARER WAS ONE OF THESE GUARDS...
        // ADDING JWT BEARER EQUIPS THIS GUARD WITH VARIOUS WEAPONS.

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
        {
            var tokenOptions = configuration.GetSection(TokenOption.Key).Get<TokenOption>();

            opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidIssuer = tokenOptions!.Issuer,
                ValidAudience = tokenOptions.Audience[0],
                IssuerSigningKey = SignService.GetSymetricKey(tokenOptions.SecurityKey),

                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateAudience = true,
                ValidateIssuer = true,
                ClockSkew = TimeSpan.Zero
            };

        }).AddCookie("ExternalCookie", opt =>
        {
            opt.Cookie.Name = "ExternalAuth";
            opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);

        }).AddOAuth("Google", config =>
        {

            config.SignInScheme = "ExternalCookie";

            // GOOGLE OPENID CONNECT URLS
            // GOOGLE USE AUTHORIZATION CODE GRANT TYPE
            config.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            config.TokenEndpoint = "https://oauth2.googleapis.com/token";
            config.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

            config.ClientId = configuration["Authentication:Google:ClientId"]!;
            config.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            config.CallbackPath = "/signin-google";
            config.SaveTokens = true;

            // SCOPES THAT REQUESTED FROM GOOGLE
            config.Scope.Add("openid");
            config.Scope.Add("email");
            config.Scope.Add("profile");

            // MAP CLAIMS THAT COME FROM GOOGLE  USER INFORMATION ENDPOINT TO OUR APPLICATION
            config.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
            config.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            config.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            config.ClaimActions.MapJsonKey("picture", "picture");

            // AFTER TOKEN EXCHANGE PULL USER INFO FROM GOOGLE
            config.Events.OnCreatingTicket = async context =>
            {
                // REQUEST GOOGLE'S USERINFO ENDPOINT
                using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                var response = await context.Backchannel.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // PARSE CLAIMS FROM JSON RESPONSE
                using var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(user.RootElement);
            };
        });

        return services;
    }
}

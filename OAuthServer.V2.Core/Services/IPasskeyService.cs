using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.Passkeys;
using OAuthServer.V2.Core.DTOs.RefreshToken;

namespace OAuthServer.V2.Core.Services;

public interface IPasskeyService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
    // THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.
    // THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

    Task<ServiceResult<PasskeyOptionsResponse>> RegisterBeginAsync(string userId);
    Task<ServiceResult> RegisterCompleteAsync(string userId, PasskeyRegisterCompleteRequest request);
    Task<ServiceResult<PasskeyOptionsResponse>> LoginBeginAsync(PasskeyLoginBeginRequest request);
    Task<ServiceResult<TokenResponse>> LoginCompleteAsync(PasskeyLoginCompleteRequest request);
}

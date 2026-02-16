using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.User;

namespace OAuthServer.V2.Core.Services;

public interface IUserService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
    // THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.
    // THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

    Task<ServiceResult<UserDto>> CreateUserAsync(SignUpRequest request);
}
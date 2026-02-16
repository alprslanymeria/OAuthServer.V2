using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(

    IUserService userService) : BaseController
{
    private readonly IUserService _userService = userService;

    [HttpPost]
    public async Task<IActionResult> CreateUser(SignUpRequest request)
        => ActionResultInstance(await _userService.CreateUserAsync(request));
}

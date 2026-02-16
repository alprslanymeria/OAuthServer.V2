using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Services;
using System.Net;

namespace OAuthServer.V2.Service.Services;

public class UserService(

    UserManager<User> userManager,
    IFileStorageHelper fileStorageHelper,
    IMapper mapper,
    ILogger<UserService> logger

    ) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IMapper _mapper = mapper;
    private readonly IFileStorageHelper _fileStorageHelper = fileStorageHelper;
    private readonly ILogger<UserService> _logger = logger;

    public async Task<ServiceResult<UserDto>> CreateUserAsync(SignUpRequest request)
    {
        // TODO: IMAGE TRANSLATION 

        // IN HERE, WE DID MANUAL MAPPING BECAUSE THE REQUEST CONTAINS IMPORTANT INFORMATION.
        var user = new User
        {
            FirstName = request.FirstName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            BirthDate = request.BirthDate,
            //Image = request.Image
            UserName = request.UserName
            
        };

        // CREATE USER WITH USER MANAGER
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // GET ERROR MESSAGES
            var errors = result.Errors.Select(e => e.Description).ToList();

            return ServiceResult<UserDto>.Fail(errors, HttpStatusCode.BadRequest);
        }

        // USER MAP TO USERDTO
        var userDto = _mapper.Map<UserDto>(user);

        return ServiceResult<UserDto>.SuccessAsCreated(userDto, $"api/user/{userDto.UserName}");
    }
}
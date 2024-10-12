using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.ResponseModels;

namespace LandsatReflectance.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    public class UserLoginInfo 
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    
    [HttpPost("Register", Name = "Register")]
    public async Task<ActionResult<User>> RegisterUser(
        [FromServices] UsersService usersService,
        [FromBody] UserLoginInfo userLoginInfo)
    {
        // TODO #1: Add validation rules for email & 

        var passwordHasher = new PasswordHasher<string>();
        var newUser = new User
        {
            Guid = Guid.NewGuid(),
            Email = userLoginInfo.Email.Trim(),
            PasswordHash = passwordHasher.HashPassword(userLoginInfo.Email, userLoginInfo.Password),
            IsEmailEnabled = true
        };

        ResponseBase<User> response = new();

        try
        {
            await usersService.TryAddUserAsync(newUser);

            response.ErrorMessage = null;
            response.Data = newUser;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }

        return ToObjectResult(response);
    }

    
    [HttpPost("Login", Name = "Login")]
    public async Task<ActionResult<string>> LoginUser(
        [FromServices] UsersService usersService,
        [FromBody] UserLoginInfo userLoginInfo)
    {
        ResponseBase<string> response = new();

        try
        {
            var user = await usersService.TryGetUserAsync(userLoginInfo.Email.Trim());

            if (user is null)
            {
                response.ErrorMessage = $"Could not find the user with the email \"{userLoginInfo.Email}\".";
                return NotFound(response);
            }
            
            var passwordHasher = new PasswordHasher<string>();
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(userLoginInfo.Email, user.PasswordHash, userLoginInfo.Password);
            
            if (passwordVerificationResult is PasswordVerificationResult.Failed)
            {
                response.ErrorMessage = $"Invalid credentials.";
                return Unauthorized(response);
            }

            response.ErrorMessage = null;
            response.Data = AuthenticationService.GenerateJwtToken(user);
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }

        return ToObjectResult(response);
    }
    
    
    private ObjectResult ToObjectResult<T>(ResponseBase<T> responseBase) 
        where T : class
    {
        return responseBase.ErrorMessage is not null
            ? StatusCode(500, responseBase)
            : Ok(responseBase);
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.ResponseModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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
    public async Task<IActionResult> RegisterUser(
        [FromServices] IOptions<JsonOptions> jsonOptions,
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
            await usersService.TryAddUser(newUser);

            response.ErrorMessage = null;
            response.Data = newUser;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }

        Func<object, ObjectResult> toObjectResult = response.ErrorMessage is not null
            ? data => StatusCode(500, data)
            : Ok;

        return toObjectResult(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
    }

    
    [HttpPost("Login", Name = "Login")]
    public async Task<IActionResult> LoginUser(
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsersService usersService,
        [FromBody] UserLoginInfo userLoginInfo)
    {
        ResponseBase<string> response = new();

        try
        {
            var user = await usersService.TryGetUser(userLoginInfo.Email.Trim());

            if (user is null)
            {
                response.ErrorMessage = $"Could not find the user with the email \"{userLoginInfo.Email}\".";
                return NotFound(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }
            
            var passwordHasher = new PasswordHasher<string>();
            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(userLoginInfo.Email, user.PasswordHash, userLoginInfo.Password);
            
            if (passwordVerificationResult is PasswordVerificationResult.Failed)
            {
                response.ErrorMessage = $"Invalid credentials.";
                return Unauthorized(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }

            response.ErrorMessage = null;
            response.Data = AuthenticationService.GenerateJwtToken(user);
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }

        Func<object, ObjectResult> toObjectResult = response.ErrorMessage is not null
            ? data => StatusCode(500, data)
            : Ok;

        return toObjectResult(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
    }
}
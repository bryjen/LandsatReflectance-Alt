using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Common.Models;
using LandsatReflectance.Common.Models.ResponseModels;
using Microsoft.Extensions.Options;
using UserLoginInfo = LandsatReflectance.Common.Models.Request.UserLoginInfo;

namespace LandsatReflectance.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    
    public AuthenticationController(IOptions<JsonOptions> jsonOptions)
    {
        m_jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
    }
    
    
    [HttpPost("Register", Name = "Register")]
    public async Task<ActionResult<User>> RegisterUser(
        [FromServices] UsersService usersService,
        [FromBody] UserLoginInfo userLoginInfo)
    {
        ResponseBase<UserWithToken> response = new();
        
        if (!IsEmailValid(userLoginInfo.Email))
        {
            response.ErrorMessage = $"The email \"{userLoginInfo.Email}\" is not valid.";
            return BadRequest(response);
        }
        
        if (!IsPasswordValid(userLoginInfo.Password))
        {
            response.ErrorMessage = $"The password \"{userLoginInfo.Password}\" is not valid.\n - The password should be at least 8 characters long.";
            return BadRequest(response);
        }
        
        var passwordHasher = new PasswordHasher<string>();
        var newUser = new User
        {
            Guid = Guid.NewGuid(),
            Email = userLoginInfo.Email.Trim(),
            PasswordHash = passwordHasher.HashPassword(userLoginInfo.Email, userLoginInfo.Password),
            IsEmailEnabled = true
        };

        try
        {
            await usersService.TryAddUserAsync(newUser);

            response.ErrorMessage = null;
            response.Data = new UserWithToken
            {
                User = newUser,
                Token = AuthenticationService.GenerateJwtToken(newUser)
            };
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
        ResponseBase<UserWithToken> response = new();

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
            response.Data = new UserWithToken
            {
                User = user,
                Token = AuthenticationService.GenerateJwtToken(user)
            };
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
            : StatusCode(200, JsonSerializer.Serialize(responseBase, m_jsonSerializerOptions));
    }
    
    // see 'https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address'
    private static bool IsEmailValid(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }    
    }

    private static bool IsPasswordValid(string password)
    {
        return password.Length >= 8;
    }
}
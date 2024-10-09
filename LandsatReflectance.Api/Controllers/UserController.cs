using System.Security.Claims;
using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Api.Controllers;

#if !DISABLE_AUTHENTICATION
[Authorize]
#endif
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    public class AddTargetsRequest
    {
        public Target[] Targets { get; set; } = [];
    }
    
    [HttpPost("AddTargets", Name = "AddTargets")]
    public async Task<IActionResult> AddTargets(
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsersService usersService,
        [FromServices] TargetsService targetsService,
        [FromBody] AddTargetsRequest addTargetsRequest)
    {
#if !DISABLE_AUTHENTICATION
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticationService.AuthenticateToken(usersService, identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }
#else
        User user = Backend.Models.User.AdminUser;
#endif
        
        ResponseBase<List<Target>> response = new();
        var targetsList = addTargetsRequest.Targets.ToList();

        try
        {
            await targetsService.AddTargets(user.Guid, targetsList);
            
            response.ErrorMessage = null;
            response.Data = targetsList;
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
    
    
    [HttpGet("GetTargets", Name = "GetTarget")]
    public async Task<IActionResult> GetTargets(
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsersService usersService,
        [FromServices] TargetsService targetsService)
    {
#if !DISABLE_AUTHENTICATION
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticationService.AuthenticateToken(usersService, identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }
#else
        User user = Backend.Models.User.AdminUser;
#endif
        
        ResponseBase<List<Target>> response = new();

        try
        {
            var targets = await targetsService.GetTargetsByUserEmail(user.Email);
            
            response.ErrorMessage = null;
            response.Data = targets;
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
    
    
    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public IActionResult DeleteTarget()
    {
        throw new NotImplementedException();
    }
    
    [HttpPatch("EditTarget", Name = "EditTarget")]
    public IActionResult EditTarget()
    {
        throw new NotImplementedException();
    }
}
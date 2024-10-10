using System.Security.Claims;
using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        return ToObjectResult(jsonOptions.Value.JsonSerializerOptions, response);
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
        
        return ToObjectResult(jsonOptions.Value.JsonSerializerOptions, response);
    }
    
    
    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public async Task<IActionResult> DeleteTarget(
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsersService usersService,
        [FromServices] TargetsService targetsService,
        [FromBody] Guid targetGuidToDelete)
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
        ResponseBase<Target> response = new();
        
        try
        {
            var targets = await targetsService.GetTargetsByUserEmail(user.Email);
            
            if (targets.FirstOrDefault(target => target.Guid == targetGuidToDelete) is null)
            {
                response.ErrorMessage = $"The user \"{user.Email}\" (guid: \"{user.Guid}\") does not have the target \"{targetGuidToDelete}\".";
                response.Data = null;
                return NotFound(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }
            
            var deletedTarget = await targetsService.DeleteTargetByGuid(targetGuidToDelete);
            
            response.ErrorMessage = null;
            response.Data = deletedTarget;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }
        
        return ToObjectResult(jsonOptions.Value.JsonSerializerOptions, response);
    }


    [HttpPatch("EditTarget", Name = "EditTarget")]
    public async Task<IActionResult> EditTarget(
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsersService usersService,
        [FromServices] TargetsService targetsService,
        [FromQuery] Guid targetGuid,
        [FromBody] JsonPatchDocument<Target> jsonPatchDoc)
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
        
        ResponseBase<Target> response = new();
        
        try
        {
            var targetToEdit = (await targetsService.GetTargetsByUserEmail(user.Email))
                .FirstOrDefault(target => target.Guid == targetGuid);
            
            if (targetToEdit is null)
            {
                response.ErrorMessage = $"The user \"{user.Email}\" (guid: \"{user.Guid}\") does not have the target \"{targetGuid}\".";
                response.Data = null;
                return NotFound(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }

            var uneditedTargetGuid = targetToEdit.Guid;
            
            jsonPatchDoc.ApplyTo(targetToEdit, jsonPathError =>
            {
                ModelState.AddModelError("", jsonPathError.ErrorMessage);
            });

            if (!ModelState.IsValid)
            {
                var invalidModelStateResponse = new ResponseBase<ModelStateDictionary>
                {
                    ErrorMessage = "There was an error editing the target, the model state is invalid.",
                    Data = ModelState
                };
                return BadRequest(JsonSerializer.Serialize(invalidModelStateResponse, jsonOptions.Value.JsonSerializerOptions));
            }

            var editedTarget = targetToEdit;  // re-assign for readability
            if (editedTarget.Guid != uneditedTargetGuid)
            {
                response.ErrorMessage = "Attempted to change the guid of the target. This is not permitted";
                response.Data = null;
                return BadRequest(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }

            if (await targetsService.ReplaceTarget(editedTarget) is null)
            {
                response.ErrorMessage = "Edit target failed.";
                response.Data = null;
                return StatusCode(500, JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
            }
            
            response.ErrorMessage = null;
            response.Data = editedTarget;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }
        
        return ToObjectResult(jsonOptions.Value.JsonSerializerOptions, response);
    }
    
    
    private ObjectResult ToObjectResult<T>(JsonSerializerOptions jsonSerializerOptions, ResponseBase<T> responseBase) 
        where T : class
    {
        var serializedData = JsonSerializer.Serialize(responseBase, jsonSerializerOptions);
        return responseBase.ErrorMessage is not null
            ? StatusCode(500, serializedData)
            : Ok(serializedData);
    }
}
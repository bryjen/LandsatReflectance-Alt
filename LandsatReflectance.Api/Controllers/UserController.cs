using System.Security.Claims;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    public async Task<ActionResult<IEnumerable<Target>>> AddTargets(
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
            await targetsService.TryAddTargetsAsync(user.Guid, targetsList);
            
            response.ErrorMessage = null;
            response.Data = targetsList;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }

        return ToObjectResult(response);
    }

    
    [HttpGet("GetTargets", Name = "GetTarget")]
    public async Task<ActionResult<IEnumerable<Target>>> GetTargets(
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
            var targets = await targetsService.TryGetTargetsByUserEmailAsync(user.Email);
            
            response.ErrorMessage = null;
            response.Data = targets;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }
        
        return ToObjectResult(response);
    }


    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public async Task<ActionResult<Target>> DeleteTarget(
        [FromServices] UsersService usersService,
        [FromServices] TargetsService targetsService,
        [FromQuery(Name = "targetGuid")] Guid targetGuidToDelete)
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
            var targets = await targetsService.TryGetTargetsByUserEmailAsync(user.Email);
            
            if (targets.FirstOrDefault(target => target.Guid == targetGuidToDelete) is null)
            {
                response.ErrorMessage = $"The user \"{user.Email}\" (guid: \"{user.Guid}\") does not have the target \"{targetGuidToDelete}\".";
                response.Data = null;
                return NotFound(response);
            }
            
            var deletedTarget = await targetsService.TryDeleteTargetByGuidAsync(targetGuidToDelete);
            
            response.ErrorMessage = null;
            response.Data = deletedTarget;
        }
        catch (Exception exception)
        {
            response.ErrorMessage = exception.Message;
            response.Data = null;
        }
        
        return ToObjectResult(response);
    }


    [HttpPatch("EditTarget", Name = "EditTarget")]
    public async Task<ActionResult<Target>> EditTarget(
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
            var targetToEdit = (await targetsService.TryGetTargetsByUserEmailAsync(user.Email))
                .FirstOrDefault(target => target.Guid == targetGuid);
            
            if (targetToEdit is null)
            {
                response.ErrorMessage = $"The user \"{user.Email}\" (guid: \"{user.Guid}\") does not have the target \"{targetGuid}\".";
                response.Data = null;
                return NotFound(response);
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
                return BadRequest(invalidModelStateResponse);
            }

            var editedTarget = targetToEdit;  // re-assign for readability
            if (editedTarget.Guid != uneditedTargetGuid)
            {
                response.ErrorMessage = "Attempted to change the guid of the target. This is not permitted";
                response.Data = null;
                return BadRequest(response);
            }

            if (await targetsService.TryReplaceTargetAsync(editedTarget) is null)
            {
                response.ErrorMessage = "Edit target failed.";
                response.Data = null;
                return StatusCode(500, response);
            }
            
            response.ErrorMessage = null;
            response.Data = editedTarget;
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
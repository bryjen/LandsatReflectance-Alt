using Microsoft.AspNetCore.Mvc;

namespace LandsatReflectance.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsgsImageController : ControllerBase
{
    [HttpGet("GetScenes", Name = "GetScenes")]
    public IActionResult GetScenes(
        [FromQuery] int path,
        [FromQuery] int row,
        [FromQuery] double minCloudCover,
        [FromQuery] double maxCloudCover,
        [FromQuery] int numResults)
    {
        throw new NotImplementedException();
    }
}
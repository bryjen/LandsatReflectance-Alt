using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Backend.Models.ResponseModels;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JsonException = Newtonsoft.Json.JsonException;

namespace LandsatReflectance.Api.Controllers;


public class ImageData
{
    public required string WholeImageUri { get; set; } = string.Empty;
    public required string PixelImageUriTemplate { get; set; } = string.Empty;
    
    public required DateTime AcquisitionDate { get; set; } = DateTime.MinValue;
    public required DateTime PublishDate { get; set; } = DateTime.MinValue;
    public required int CloudCover { get; set; }
}


[ApiController]
[Route("[controller]")]
public class UsgsImageController : ControllerBase
{
    [HttpGet("GetScenes", Name = "GetScenes")]
    public async Task<IActionResult> GetScenes(
        [FromServices] ILogger<UsgsImageController> logger,
        [FromServices] IOptions<JsonOptions> jsonOptions,
        [FromServices] UsgsImageService usgsImageService,
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row,
        [FromQuery(Name = "numResults")] int numResults = 5,
        [FromQuery(Name = "minCloudCover")] double minCloudCover = 0,
        [FromQuery(Name = "maxCloudCover")] double maxCloudCover = 100,
        [FromQuery(Name = "includeUnknownCloudCover")] bool includeUnknownCloudCover = true)
    {
        var sceneSearchRequest = CreatePathRowSceneSearchRequest(path, row, numResults, minCloudCover, maxCloudCover, includeUnknownCloudCover);

        try
        {
            var sceneSearchResponse = await usgsImageService.QuerySceneSearch(sceneSearchRequest);
            var sceneSearchData = sceneSearchResponse.Data;
            var sceneDataArr = sceneSearchData?.ReturnedSceneData.ToArray() ?? [];

            IEnumerable<ImageData> asImageData = sceneDataArr
                .Select(ToImageData)
                .Where(imageData => imageData is not null)
                .OrderByDescending(imageData => imageData!.PublishDate)!;

            var response = new ResponseBase<IEnumerable<ImageData>>
            {
                ErrorMessage = null,
                Data = asImageData
            };

            return Ok(JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or TaskCanceledException)
        {
            const string errorMsg = "There was a problem fetching image data with code.";
            var response = new ResponseBase<object>
            {
                ErrorMessage = errorMsg,
                Data = null 
            };
            
            logger.LogError($"[{exception.GetHashCode()}] {errorMsg} The operation failed with error message: \"{exception.Message}\"");
            logger.LogTrace($"[{exception.GetHashCode()}] Stack trace:\n{exception.StackTrace}");
            return StatusCode(500, JsonSerializer.Serialize(response, jsonOptions.Value.JsonSerializerOptions));
        }
    }

    
    private static ImageData? ToImageData(SceneData sceneData)
    {
        if (sceneData.BrowseInfos.Length <= 0 || sceneData.TemporalCoverage is null)
        {
            return null;
        }

        return new ImageData
        {
            WholeImageUri = sceneData.BrowseInfos[0].BrowsePath,
            PixelImageUriTemplate = sceneData.BrowseInfos[0].OverlayPath,
            
            PublishDate = sceneData.PublishDate,
            AcquisitionDate = sceneData.TemporalCoverage.StartDate,
            CloudCover = sceneData.CloudCover
        };
    }
    
    private static SceneSearchRequest CreatePathRowSceneSearchRequest(int path, int row, int numResults,
        double minCloudCover, double maxCloudCover, bool includeUnknownCloudCover)
    {
        var metadataFilter = new MetadataFilterAnd 
        {
            ChildFilters = [
                new MetadataFilterValue  // Path filer
                {
                    FilterId = "5e83d14fb9436d88",
                    Value = path.ToString(),
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                },
                new MetadataFilterValue  // Row filter
                {
                    FilterId = "5e83d14ff1eda1b8",
                    Value = row.ToString(),
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                }
            ] 
        };

        var cloudCoverFilter = new CloudCoverFilter
        {
            Min = (int)Math.Floor(minCloudCover),
            Max = (int)Math.Ceiling(maxCloudCover),
            IncludeUnknown = includeUnknownCloudCover,
        };
        
        var sceneFilter = new SceneFilter
        {
            MetadataFilter = metadataFilter,
            CloudCoverFilter = cloudCoverFilter
        };
        
        return new SceneSearchRequest
        {
            DatasetName = UsgsImageService.DatasetName,
            MaxResults = numResults,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };
    }}
using System.Text;
using System.Text.Json;
using LandsatReflectance.Api.Models.UsgsApi;
using LandsatReflectance.Api.Models.UsgsApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace LandsatReflectance.Api.Services;


public class UsgsImageService
{
    public static readonly string DatasetName = "landsat_ot_c2_l2";
    
    private readonly ILogger<UsgsImageService> m_logger;
    
    private readonly HttpClient m_httpClient;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    
    public UsgsImageService(ILogger<UsgsImageService> logger, HttpClient httpClient, IOptions<JsonOptions> jsonOptions)
    {
        m_logger = logger;
        m_httpClient = httpClient; 
        m_jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
    }
    
    
    public async Task<UsgsApiResponse<SceneSearchResponse>> QuerySceneSearch(SceneSearchRequest sceneSearchRequest)
    {
        string asJson = JsonSerializer.Serialize(sceneSearchRequest, m_jsonSerializerOptions);
        return await QueryAsync<SceneSearchResponse>(m_jsonSerializerOptions, m_httpClient, "scene-search", asJson);
    }
    
    
    internal static async Task<UsgsApiResponse<TResponseType>> QueryAsync<TResponseType>(
        JsonSerializerOptions jsonSerializerOptions,
        HttpClient httpClient,
        string requestUri, 
        string requestContents) 
        where TResponseType : class, IUsgsApiResponseData
    {
        using var contents = new StringContent(requestContents, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(requestUri, contents);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        var responseContentsRaw = await response.Content.ReadAsStringAsync();
        var deserializedObject = JsonSerializer.Deserialize<UsgsApiResponse<TResponseType>>(responseContentsRaw, jsonSerializerOptions);
        
        if (deserializedObject is null)
        {
            throw new JsonException("Failed to deserialize the API response. The JSON format might be invalid or unexpected.");
        }
        
        return deserializedObject;
    }
}
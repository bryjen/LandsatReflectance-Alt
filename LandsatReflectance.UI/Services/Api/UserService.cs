using System.Text;
using System.Text.Json;
using LandsatReflectance.Common.Models.Request;
using LandsatReflectance.Common.Models.ResponseModels;

namespace LandsatReflectance.UI.Services.Api;

public class UserService
{
    private JsonSerializerOptions m_jsonSerializerOptions;
    private ILogger<UserService> m_logger;
    private HttpClient m_httpClient;
    private CurrentUserService m_currentUserService;
    
    public UserService(
        JsonSerializerOptions jsonSerializerOptions, 
        ILogger<UserService> logger, 
        HttpClient httpClient, 
        CurrentUserService currentUserService)
    {
        m_jsonSerializerOptions = jsonSerializerOptions;
        m_logger = logger;
        m_httpClient = httpClient;
        m_currentUserService = currentUserService;
    }

    public async Task Login(string email, string password)
    {
        // TODO: Refactor exception handling here
        
        var userLoginRequest = new UserLoginInfo
        {
            Email = email,
            Password = password
        };
        
        using var contents = new StringContent(JsonSerializer.Serialize(userLoginRequest, m_jsonSerializerOptions), Encoding.UTF8, "application/json");
        var response = await m_httpClient.PostAsync("Authentication/Login", contents);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var loginResponse = await JsonSerializer.DeserializeAsync<ResponseBase<string>>(stream, m_jsonSerializerOptions);

        if (loginResponse is null)
        {
            throw new Exception($"Response is null.");
        }

        if (loginResponse.ErrorMessage is not null || loginResponse.Data is null)
        {
            throw new Exception($"Error: \"{loginResponse.ErrorMessage}\"");
        }

        string authToken = loginResponse.Data;
        m_currentUserService.InitCurrentUser(email, authToken);
    }
}
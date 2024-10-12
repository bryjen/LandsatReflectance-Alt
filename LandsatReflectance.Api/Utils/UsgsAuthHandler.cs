using System.Security.Authentication;
using System.Text.Json;
using LandsatReflectance.Api.Services;
using LandsatReflectance.Api.Models.UsgsApi.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Api.Utils;

public class UsgsAuthHandler : DelegatingHandler
{
    private ILogger<UsgsAuthHandler> m_logger;
    private JsonSerializerOptions m_jsonSerializerOptions;
    private UsgsAuthTokenStore m_usgsAuthTokenStore;
    
    
    public UsgsAuthHandler(ILogger<UsgsAuthHandler> logger, IOptions<JsonOptions> jsonOptions, UsgsAuthTokenStore usgsAuthTokenStore)
    {
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;

        m_usgsAuthTokenStore = usgsAuthTokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (m_usgsAuthTokenStore.Token is null || m_usgsAuthTokenStore.IsExpired)
        {
            var newAuthToken = await TryGetAuthTokenAsync();
            
            if (newAuthToken is null)
            {
                const string errorMsg = "Could not get authentication token.";
                m_logger.LogCritical(errorMsg);
                throw new AuthenticationException(errorMsg);
            }

            m_usgsAuthTokenStore.Token = newAuthToken;
        }
        
        request.Headers.Add("X-Auth-Token", m_usgsAuthTokenStore.Token);
        
        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string?> TryGetAuthTokenAsync()
    {
        using var tempHttpClient = new HttpClient();
        tempHttpClient.BaseAddress = new Uri("https://m2m.cr.usgs.gov/api/api/json/stable/");
        
        var loginTokenRequest = new LoginTokenRequest
        {
            Username = EnvironmentVariablesService.UsgsUsername,
            Token = EnvironmentVariablesService.UsgsAppToken
        };        
        string asJson = JsonSerializer.Serialize(loginTokenRequest, m_jsonSerializerOptions);
        var response = await UsgsImageService.QueryAsync<LoginTokenResponse>(m_jsonSerializerOptions, tempHttpClient, "login-token", asJson);;

        if (response.ErrorCode is not null)
        {
            throw new InvalidOperationException($"The USGS m2m API response returned an error code \"{response.ErrorCode}\" with message \"{response.ErrorMessage}\".");
        }

        var loginTokenResponse = response.Data;
        if (loginTokenResponse is null)
        {
            throw new InvalidOperationException($"The USGS m2m API response dit not return an error code, but had no data.");
        }

        return loginTokenResponse.AuthToken;
    }
}
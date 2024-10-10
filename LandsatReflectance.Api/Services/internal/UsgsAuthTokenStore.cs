namespace LandsatReflectance.Api.Services;

// Intended to be a singleton service to keep track of the current USGS M2M API authentication token.

public class UsgsAuthTokenStore
{
    private static readonly TimeSpan TokenDuration = TimeSpan.FromHours(2);
    private static readonly TimeSpan ExpiryAllowance = TimeSpan.FromMinutes(5);
    
    private string? m_token = null;
    private DateTime? m_expiry = null;

    
    public string? Token
    {
        get => m_token;
        set
        {
            m_token = value;
            m_expiry = DateTime.UtcNow + TokenDuration;
        }
    } 

    public bool IsExpired => m_expiry is null || DateTime.UtcNow >= m_expiry - ExpiryAllowance;
}
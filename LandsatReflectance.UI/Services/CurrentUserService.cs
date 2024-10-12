namespace LandsatReflectance.UI.Services;

public class CurrentUserService
{
    public bool IsLoggedIn { get; private set; } = false;
    public string Email { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    
    private ILogger<CurrentUserService> m_logger;
    
    public CurrentUserService(ILogger<CurrentUserService> logger)
    {
        m_logger = logger;
    }

    public void InitCurrentUser(string email, string authToken)
    {
        IsLoggedIn = true;
        Email = email;
        AuthToken = authToken;
    }
}
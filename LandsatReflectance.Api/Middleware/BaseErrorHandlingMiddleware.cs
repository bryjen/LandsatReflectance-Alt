namespace LandsatReflectance.Api.Middleware;

public class BaseErrorHandlingMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly ILogger<BaseErrorHandlingMiddleware> m_logger;
    
    public BaseErrorHandlingMiddleware(RequestDelegate next, ILogger<BaseErrorHandlingMiddleware> logger)
    {
        m_next = next;
        m_logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await m_next(httpContext);
        }
        catch (Exception exception)
        {
            m_logger.LogCritical($"[{exception.GetHashCode()}] An unhandled exception occurred and caught by the general error handling middleware.");
            m_logger.LogCritical($"[{exception.GetHashCode()}] {exception}");
        }
    }
}
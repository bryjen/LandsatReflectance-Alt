using LandsatReflectance.Api.Exceptions;

namespace LandsatReflectance.Api.Services;

public static class EnvironmentVariablesService
{
    public static string UsgsUsername = null!;
    public static string UsgsAppToken = null!;
    
    public static string AuthSecretKey = null!;
    public static string DbConnectionString = null!;


    /// <summary>
    /// This function is meant to be called at 'Program.cs' to initialize the loading of environment variables. Normally,
    /// they would be initialized on first access, which could cause errors when the program is actually running.
    /// </summary>
    public static void Init()
    { }
    
    static EnvironmentVariablesService()
    {
        var missingEnvironmentVariables = new List<string>();
        Action<string> onValueNotFound = envVarName => missingEnvironmentVariables.Add(envVarName);

        TryGetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_USERNAME", value => UsgsUsername = value, onValueNotFound);
        TryGetEnvironmentVariable("LANDSAT_REFLECTANCE_USGS_TOKEN", value => UsgsAppToken = value, onValueNotFound);
        
        TryGetEnvironmentVariable("FLAT_EARTHERS_AUTH_SECRET_KEY", value => AuthSecretKey = value, onValueNotFound);
        TryGetEnvironmentVariable("FLAT_EARTHERS_DB_CONNECTION_STRING", value => DbConnectionString = value, onValueNotFound);

        
        if (missingEnvironmentVariables.Any())
        {
            throw new MissingEnvironmentVariablesException(missingEnvironmentVariables);
        }
    }

    private static void TryGetEnvironmentVariable(string envVarName, Action<string> onValueFound,
        Action<string> onValueNotFound)
    {
        var value = Environment.GetEnvironmentVariable(envVarName);
        if (value is not null)
        {
            onValueFound(value);
        }
        else
        {
            onValueNotFound(envVarName);
        }
    }
}
namespace LandsatReflectance.Api.Exceptions;

public class MissingEnvironmentVariablesException : Exception
{
    public MissingEnvironmentVariablesException(IEnumerable<string> missingEnvironmentVariables) 
        : base(FormatIntoString(missingEnvironmentVariables))
    { }

    private static string FormatIntoString(IEnumerable<string> missingEnvironmentVariables)
    {
        string listOfEnvVariables = string.Join(", ", missingEnvironmentVariables.Select(str => $"\"{str}\""));
        return $"The following environment variables are missing: {listOfEnvVariables}";
    }
}
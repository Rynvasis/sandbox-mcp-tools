namespace PlatziStore.Shared.Exceptions;

public class ConfigurationMissingException : Exception
{
    public string ConfigKey { get; }

    public ConfigurationMissingException(string configKey) 
        : base($"Required configuration key '{configKey}' is missing or invalid.")
    {
        ConfigKey = configKey;
    }
}

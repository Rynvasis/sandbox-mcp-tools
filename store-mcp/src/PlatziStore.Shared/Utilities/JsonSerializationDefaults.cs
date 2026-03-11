using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatziStore.Shared.Utilities;

public static class JsonSerializationDefaults
{
    public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

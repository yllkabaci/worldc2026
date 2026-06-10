using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorldCup.Api.Common.Serialization;

/// <summary>Shared JSON options: camelCase, omit nulls, enums as strings. Never construct ad-hoc options.</summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Api = Create();

    public static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

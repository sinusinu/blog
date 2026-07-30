using System.Text.Json;
using System.Text.Json.Serialization;

namespace Siblsenki;

public class DynamicTarget {
    [JsonPropertyName("type")] public required string Type { get; set; }
    [JsonPropertyName("in")] public required string In { get; set; }
    [JsonPropertyName("out")] public required string Out { get; set; }
    [JsonPropertyName("options")] public Dictionary<string, JsonElement>? Options { get; set; }
}
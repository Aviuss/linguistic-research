using System;

namespace phylogenetic_project.Persistance;

using System.Collections.Concurrent;
using System.Text.Json.Serialization;

public class LanguageRules
{
    [JsonPropertyName("idb_compatible")]
    public int[] IdbCompatible { get; set; } = Array.Empty<int>();

    [JsonPropertyName("info")]
    public string Info { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    [JsonConverter(typeof(LanguageRulesArrayConverter))]
    public ConcurrentDictionary<string, string[]> Rules { get; set; } = new();
}

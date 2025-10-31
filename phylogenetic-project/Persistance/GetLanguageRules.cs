using System;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class GetLanguageRules
{
    public static List<LanguageRules>? ReadFromFile()
    {
        string json = File.ReadAllText(Path.Combine(Program.dataAndResultsPath, "json settings", "ipa_rules.json"));

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        List<LanguageRules>? data = JsonSerializer.Deserialize<List<LanguageRules>>(json, options);
        return data;
    }
}

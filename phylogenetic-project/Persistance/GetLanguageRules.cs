using System;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class GetLanguageRules
{
    public static LanguageRules[] ReadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        LanguageRules[]? data = JsonSerializer.Deserialize<LanguageRules[]>(json, options);
        
        if (data == null)
        {
            throw new Exception("Couldn't parse ipa_rules.json");
        }

        return data;
    }
}

using System;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class LanguageRulesWrapper
{
    public string resourceId;
    public LanguageRules[] languageRules;

    public LanguageRulesWrapper(string filePath, string resourceId)
    {
        this.resourceId = resourceId;
        languageRules = ReadFromFile(filePath);
    }

    private LanguageRules[] ReadFromFile(string filePath)
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

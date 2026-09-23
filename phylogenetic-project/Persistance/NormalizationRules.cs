using System;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class NormalizationRules
{
    public const int MaxPasses = 1000;

    public string resourceId;
    public IReadOnlyList<(string from, string to)> rules;

    public NormalizationRules(string filePath, string resourceId)
        : this(ReadFromFile(filePath), resourceId)
    {
    }

    public NormalizationRules(IEnumerable<(string from, string to)> rules, string resourceId)
    {
        this.resourceId = resourceId;
        this.rules = rules.ToList();
        Validate(this.rules);
    }

    public string Apply(string text)
    {
        for (int pass = 0; pass < MaxPasses; pass++)
        {
            string before = text;
            foreach (var (from, to) in rules)
            {
                text = text.Replace(from, to, StringComparison.Ordinal);
            }

            if (text == before)
            {
                return text;
            }
        }

        throw new Exception($"Normalization rules \"{resourceId}\" did not stabilize after {MaxPasses} passes. Check for rules that undo or feed each other (e.g. \"a\" -> \"aa\").");
    }

    private static List<(string from, string to)> ReadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        string[][]? data = JsonSerializer.Deserialize<string[][]>(json, options);

        if (data == null)
        {
            throw new Exception("Couldn't parse normalization rules json");
        }

        return data.Select((pair, i) =>
        {
            if (pair == null || pair.Length != 2 || pair[0] == null || pair[1] == null)
            {
                throw new Exception($"Normalization rule #{i} must be a [\"from\", \"to\"] pair of strings.");
            }
            return (pair[0], pair[1]);
        }).ToList();
    }

    private static void Validate(IReadOnlyList<(string from, string to)> rules)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < rules.Count; i++)
        {
            if (string.IsNullOrEmpty(rules[i].from))
            {
                throw new Exception($"Normalization rule #{i} has an empty \"from\" string.");
            }

            if (!seen.Add(rules[i].from))
            {
                throw new Exception($"Normalization rule #{i}: \"{rules[i].from}\" is defined more than once. Each \"from\" must map to exactly one \"to\".");
            }
        }
    }
}

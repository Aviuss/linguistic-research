using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class MapIdbToName
{
    public static ConcurrentDictionary<int, string>? ReadFromFile()
    {
        string filePath = Path.Combine(Program.dataAndResultsPath, "json settings", "map_idb_to_name.json");
        string json = File.ReadAllText(filePath);

        var result = JsonSerializer.Deserialize<ConcurrentDictionary<int, string>>(json);
        
        return result;
    }
}

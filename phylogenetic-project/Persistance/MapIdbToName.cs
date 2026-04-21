using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace phylogenetic_project.Persistance;

public class MapIdbToName
{
    public static ConcurrentDictionary<int, string> ReadFromFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var result = JsonSerializer.Deserialize<ConcurrentDictionary<int, string>>(json);
        
        if (result == null)
        {
            throw new Exception("Couldn't parse map_idb_to_name.json");
        }

        return result;
    }
}

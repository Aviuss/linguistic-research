using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace phylogenetic_project.Persistance;


public class GetChapterFromJson : IGetChapter
{
    public string resourceId => _resourceId;
    private string _resourceId = null!;
    
    private ConcurrentDictionary<(int, int), string> dictionary = new();

    public GetChapterFromJson(string dataPath, string resourceId)
    {
        this._resourceId = resourceId;
        string json = File.ReadAllText(dataPath);

        List<ChapterCustomDataTextEntry> entries = JsonSerializer.Deserialize<List<ChapterCustomDataTextEntry>>(json)
            ?? throw new InvalidOperationException("Deserialization returned null.");


        foreach (var entry in entries)
        {
            dictionary.TryAdd((entry.idb, entry.chapter), entry.text);
        }
    }

    public string GetChapter(int bookIDB, int chapterNo)
    {
        dictionary.TryGetValue((bookIDB, chapterNo), out string? value);
        ArgumentNullException.ThrowIfNull(value);

        return value;
    }

    private class ChapterCustomDataTextEntry
    {
        public int idb { get; set; }
        public int chapter { get; set; }
        public string text { get; set; } = "";
    }

}
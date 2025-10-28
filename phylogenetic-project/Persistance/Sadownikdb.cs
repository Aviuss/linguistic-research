using System;
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
namespace phylogenetic_project.Persistance;

public class Sadownikdb: IDisposable
{
    public SqliteConnection connection;
    private bool _disposed = false;

    public List<int> BookIDBs = new List<int>();
    public List<int> Chapters = new List<int>();

    private readonly ConcurrentDictionary<CacheKey, int> chapterLengthCache = new ConcurrentDictionary<CacheKey, int>();



    public Sadownikdb(string dbPath)
    {
        if (!File.Exists(dbPath))
            throw new FileNotFoundException("Database file not found!", dbPath);

        connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        InitBookIds();
        InitChapters();
    }

    public void SetChapterLengthCache(int key1, int key2, int value)
    {
        var cacheKey = new CacheKey(key1, key2);
        chapterLengthCache[cacheKey] = value;
    }

    public bool TryGetChapterLengthCache(int key1, int key2, out int value)
    {
        var cacheKey = new CacheKey(key1, key2);
        return chapterLengthCache.TryGetValue(cacheKey, out value);
    }

    public void ClearChapterLengthCache()
    {
        chapterLengthCache.Clear();
    }

    public string GetChapter(int bookIDB, int chapterNo)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            select
            SENTENCE.TEXT
            from SENTENCE
            where
            SENTENCE.IDB = $bookIDB AND
            SENTENCE.CHAPTER = $chapterNo
            ORDER BY SENTENCE.CHAPTER, SENTENCE.LINE ASC
         """;
        command.Parameters.AddWithValue("$bookIDB", bookIDB);
        command.Parameters.AddWithValue("$chapterNo", chapterNo);

        var lines = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            lines.Add(reader.GetString(0));
        }

        string text = string.Join(" ", lines);
        SetChapterLengthCache(bookIDB, chapterNo, text.Length);
        return text;
    }

    public void InitBookIds()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct BOOK.IDB from BOOK""";

        BookIDBs = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            BookIDBs.Add(reader.GetInt32(0));
        }
    }

    public void InitChapters()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct CHAPTER from SENTENCE""";

        Chapters = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Chapters.Add(reader.GetInt32(0));
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        connection.Dispose();
        GC.SuppressFinalize(this);
    }
}

struct CacheKey
{
    public int Key1 { get; }
    public int Key2 { get; }

    public CacheKey(int key1, int key2)
    {
        Key1 = key1;
        Key2 = key2;
    }

    public override bool Equals(object? obj) => obj is CacheKey other && Equals(other);

    public bool Equals(CacheKey other) => Key1 == other.Key1 && Key2 == other.Key2;

    public override int GetHashCode() => HashCode.Combine(Key1, Key2);
}

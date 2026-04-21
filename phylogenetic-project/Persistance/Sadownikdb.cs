using System;
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
namespace phylogenetic_project.Persistance;

public class Sadownikdb: IDisposable, IGetChapter
{
    public string resourceId => _resourceId;
    private string _resourceId = null!;


    private SqliteConnection connection;
    private bool _disposed = false;

    private List<int> BookIDBs = [];
    private List<int> Chapters = [];
    

    public Sadownikdb(string dbPath, string resourceId)
    {
        this._resourceId = resourceId;

        if (!File.Exists(dbPath))
            throw new FileNotFoundException("Database file not found!", dbPath);

        connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        InitBookIds();
        InitChapters();
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
            lines.Add(reader.GetString(0).Trim());
        }

        string text = string.Join(" ", lines);
        return text;
    }

    public void InitBookIds()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct BOOK.IDB from BOOK order by BOOK.IDB""";

        BookIDBs = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            BookIDBs.Add(reader.GetInt32(0));
        }
        BookIDBs.Sort();
    }

    public void InitChapters()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct CHAPTER from SENTENCE order by CHAPTER""";

        Chapters = new List<int>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Chapters.Add(reader.GetInt32(0));
        }
        Chapters.Sort();
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
using System;
using Microsoft.Data.Sqlite;
namespace phylogenetic_project.Persistance;

public class Sadownikdb: IDisposable
{
    public SqliteConnection connection;
    private bool _disposed = false;

    public List<Int64> BookIDBs = new List<Int64>();
    public List<Int64> Chapters = new List<Int64>();


    public Sadownikdb(string dbPath)
    {
        if (!File.Exists(dbPath))
            throw new FileNotFoundException("Database file not found!", dbPath);

        connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadOnly");
        connection.Open();

        InitBookIds();
        InitChapters();
    }

    public void InitBookIds()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct BOOK.IDB from BOOK""";

        BookIDBs = new List<Int64>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            BookIDBs.Add(reader.GetInt64(0));
        }
    }

    public void InitChapters()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """select distinct CHAPTER from SENTENCE""";

        Chapters = new List<Int64>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Chapters.Add(reader.GetInt64(0));
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

using System;
using System.Collections.Concurrent;
using Microsoft.Data.Sqlite;
namespace phylogenetic_project.Persistance;

public class CacheDB: IDisposable
{
    public SqliteConnection connection;
    private bool _disposed = false;

    public CacheDB(string dbPath)
    {
        connection = new SqliteConnection($"Data Source={dbPath};Mode=ReadWriteCreate");
        connection.Open();

        CreateSchema();
    }

    private void CreateSchema()
    {
        var command = connection.CreateCommand();
        command.CommandText =
        @"
            CREATE TABLE IF NOT EXISTS data (
                id INTEGER PRIMARY KEY,
                algorithm TEXT NOT NULL,
                algorithmArgs TEXT,
                algorithmResult TEXT NOT NULL,
                idb1 INTEGER NOT NULL,
                idb2 INTEGER NOT NULL,
                chapter INTEGER NOT NULL,
                timestamp INTEGER
            );
        ";
        command.ExecuteNonQuery();        
    }

    public void InsertCache(string algorithmName, string algorithmArgs, string algorithmResult, int idb1, int idb2, int chapter)
    {
        var command = connection.CreateCommand();
        command.CommandText =
        @"
            INSERT INTO data
                (id, algorithm, algorithmArgs, algorithmResult, idb1, idb2, chapter, timestamp)
            VALUES
                (
                    (SELECT IFNULL(MAX(id), 0) + 1 FROM data),
                    $algorithm,
                    $algorithmArgs,
                    $algorithmResult,
                    $idb1,
                    $idb2,
                    $chapter,
                    $timestamp
                );
        ";
        command.Parameters.AddWithValue("$algorithm", algorithmName);
        command.Parameters.AddWithValue("$algorithmArgs", algorithmArgs);
        command.Parameters.AddWithValue("$algorithmResult", algorithmResult);
        command.Parameters.AddWithValue("$idb1", idb1);
        command.Parameters.AddWithValue("$idb2", idb2);
        command.Parameters.AddWithValue("$chapter", chapter);
        command.Parameters.AddWithValue("$timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        command.ExecuteNonQuery();
    }
    
    public string? TryToGetFromCache(string algorithmName, string algorithmArgs, int idb1, int idb2, int chapter)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT algorithmResult FROM data 
        WHERE
            algorithm = $algorithm AND 
            algorithmArgs = $algorithmArgs AND
            idb1 = $idb1 AND
            idb2 = $idb2 AND
            chapter = $chapter;
        """;

        command.Parameters.AddWithValue("$algorithm", algorithmName);
        command.Parameters.AddWithValue("$algorithmArgs", algorithmArgs);
        command.Parameters.AddWithValue("$idb1", idb1);
        command.Parameters.AddWithValue("$idb2", idb2);
        command.Parameters.AddWithValue("$chapter", chapter);
        

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            return reader.GetString(0);
        }
        return null;
    }


    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        connection.Dispose();
        GC.SuppressFinalize(this);
    }

    internal void InsertCache(string v1, string v2, object value, int v3, int v4, int v5)
    {
        throw new NotImplementedException();
    }
}

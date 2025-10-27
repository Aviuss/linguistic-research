using System;
using Microsoft.VisualBasic;
using SQLitePCL;

namespace phylogenetic_project.Utility;

public class BookMatrix<T_FieldData>
{
    private T_FieldData[,] matrix;

    public List<Int64> BookIDBs = new List<Int64>();
    public List<Int64> Chapters = new List<Int64>();

    public BookMatrix(List<Int64> bookIDBs, List<Int64> chapters)
    {
        BookIDBs = bookIDBs;
        Chapters = chapters;
        matrix = new T_FieldData[BookIDBs.Count, Chapters.Count];
    }

    public void SetValueByListIdx(int bookIndex, int chapterIndex, T_FieldData value)
    {
        matrix[bookIndex, chapterIndex] = value;
    }

    public T_FieldData GetValueByListIdx(int bookIndex, int chapterIndex)
    {
        return matrix[bookIndex, chapterIndex];
    }

    public void SetValue(Int64 idb, Int64 chapterId, T_FieldData value)
    {
        var i = BookIDBs.FindIndex(element => element == idb);
        var j = Chapters.FindIndex(element => element == chapterId);

        if (i == -1)
            throw new ArgumentException($"BookID {idb} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterId} not found");

        matrix[i, j] = value;
    }

    public T_FieldData GetValue(Int64 idb, Int64 chapterId, T_FieldData value)
    {
        var i = BookIDBs.FindIndex(element => element == idb);
        var j = Chapters.FindIndex(element => element == chapterId);

        if (i == -1)
            throw new ArgumentException($"BookID {idb} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterId} not found");

        return matrix[i, j];
    }

}

using System;
using Microsoft.VisualBasic;
using SQLitePCL;

namespace phylogenetic_project.Utility;

public class BookMatrix<T_FieldData>
{
    private T_FieldData[,] matrix;

    public List<int> BookIDBs = new List<int>();
    public List<int> Chapters = new List<int>();

    public BookMatrix(List<int> bookIDBs, List<int> chapters)
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

    public void SetValue(int idb, int chapterNo, T_FieldData value)
    {
        var i = BookIDBs.FindIndex(element => element == idb);
        var j = Chapters.FindIndex(element => element == chapterNo);

        if (i == -1)
            throw new ArgumentException($"BookID {idb} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterNo} not found");

        matrix[i, j] = value;
    }

    public T_FieldData GetValue(int idb, int chapterNo, T_FieldData value)
    {
        var i = BookIDBs.FindIndex(element => element == idb);
        var j = Chapters.FindIndex(element => element == chapterNo);

        if (i == -1)
            throw new ArgumentException($"BookID {idb} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterNo} not found");

        return matrix[i, j];
    }

}

using System;
using Microsoft.VisualBasic;
using phylogenetic_project.Algorithms;
using phylogenetic_project.Matrices.CellChapterJobs;
using SQLitePCL;

namespace phylogenetic_project.Matrices;

public class BookMatrix<T_FieldData>
{
    private T_FieldData[,][] matrix;
    decimal[,]? result_matrix;

    public List<int> bookIDBs = new List<int>();
    public List<int> chapters = new List<int>();

    public IMatrixCellChapterJob<T_FieldData>? matrixCellChapterJob;

    public BookMatrix(List<int> bookIDBs_, List<int> chapters_, IMatrixCellChapterJob<T_FieldData>? matrixCellChapterJob_)
    {
        bookIDBs = bookIDBs_;
        chapters = chapters_;
        matrix = new T_FieldData[this.bookIDBs.Count, this.bookIDBs.Count][];
        for (int i = 0; i < bookIDBs.Count; i++)
        {
            for (int j = 0; j < bookIDBs.Count; j++)
            {
                matrix[i, j] = new T_FieldData[chapters.Count];
            }
        }

        if (matrixCellChapterJob_ != null)
        {
            matrixCellChapterJob = matrixCellChapterJob_;
            matrixCellChapterJob.bookIDBs = bookIDBs;
            matrixCellChapterJob.chapters = chapters;
        }
    }

    public decimal[,]? GetResultMatrix()
    {
        if (matrixCellChapterJob == null) { return null; }

        result_matrix = new decimal[this.bookIDBs.Count, this.bookIDBs.Count];

        for (int idx_idb1 = 0; idx_idb1 < bookIDBs.Count; idx_idb1++)
        {
            for (int idx_idb2 = idx_idb1 + 1; idx_idb2 < bookIDBs.Count; idx_idb2++)
            {
                for (int idx_chapter = 0; idx_chapter < chapters.Count; idx_chapter++)
                {
                    matrix[idx_idb1, idx_idb2][idx_chapter] = matrixCellChapterJob.Calculate(idx_idb1, idx_idb2, idx_chapter);
                    matrix[idx_idb2, idx_idb1][idx_chapter] = matrix[idx_idb1, idx_idb2][idx_chapter];
                }

            }
        }


        for (int idx_idb1 = 0; idx_idb1 < bookIDBs.Count; idx_idb1++)
        {
            for (int idx_idb2 = idx_idb1; idx_idb2 < bookIDBs.Count; idx_idb2++)
            {
                if (idx_idb1 == idx_idb2)
                {
                    result_matrix[idx_idb1, idx_idb2] = 0;
                } else
                {
                    result_matrix[idx_idb1, idx_idb2] = matrixCellChapterJob.MergeChapters(matrix[idx_idb1, idx_idb2]);
                    result_matrix[idx_idb2, idx_idb1] = result_matrix[idx_idb1, idx_idb2];
                }
            }
        }


        return result_matrix;
    }



    public void SetValueByListIdx(int bookIndex1, int bookIndex2, int chapterIndex, T_FieldData value)
    {
        matrix[bookIndex1, bookIndex2][chapterIndex] = value;
        matrix[bookIndex2, bookIndex1][chapterIndex] = value;
    }

    public T_FieldData GetValueByListIdx(int bookIndex1, int bookIndex2, int chapterIndex)
    {
        return matrix[bookIndex1, bookIndex2][chapterIndex];
    }

    public void SetValue(int idb1, int idb2, int chapterNo, T_FieldData value)
    {
        var i1 = bookIDBs.FindIndex(element => element == idb1);
        var i2 = bookIDBs.FindIndex(element => element == idb2);
        var j = chapters.FindIndex(element => element == chapterNo);

        if (i1 == -1)
            throw new ArgumentException($"BookID {idb1} not found");
        
        if (i2 == -1)
            throw new ArgumentException($"BookID {idb2} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterNo} not found");

        matrix[i1, i2][j] = value;
        matrix[i2, i1][j] = value;
    }

    public T_FieldData GetValue(int idb1, int idb2, int chapterNo, T_FieldData value)
    {
        var i1 = bookIDBs.FindIndex(element => element == idb1);
        var i2 = bookIDBs.FindIndex(element => element == idb2);
        var j = chapters.FindIndex(element => element == chapterNo);

        if (i1 == -1)
            throw new ArgumentException($"BookID {idb1} not found");

        if (i2 == -1)
            throw new ArgumentException($"BookID {idb2} not found");

        if (j == -1)
            throw new ArgumentException($"ChapterID {chapterNo} not found");

        return matrix[i1, i2][j];
    }

}

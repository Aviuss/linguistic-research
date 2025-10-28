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

}

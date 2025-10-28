using System;
using iluvadev.ConsoleProgressBar;
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

    public BookMatrix(
        List<int> bookIDBs_,
        List<int> chapters_,
        IMatrixCellChapterJob<T_FieldData>? matrixCellChapterJob_)
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

    public decimal[,]? CalculateResultMatrix(bool showProgressBar = true)
    {
        using var progressBar = InitProgressBar(showProgressBar);

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
                    
                    progressBar?.PerformStep(1, $"matrixCellChapterJob.Calculate(idb_{idx_idb1}, idb_{idx_idb2}, chap_{idx_chapter})");
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

        if (progressBar != null)
        {
            Console.Write("\n\n\n");
        }

        return result_matrix;
    }

    public ProgressBar? InitProgressBar(bool showProgressBar)
    {
        if (showProgressBar == false)
        {
            return null;
        }

        var max = bookIDBs.Count * (bookIDBs.Count - 1) / 2 * chapters.Count;
        var progressBar = new ProgressBar() { Maximum = max };
        progressBar.Delay = 333;

        progressBar.Text.Description.Clear();
        progressBar.Text.Description.Processing.AddNew().SetValue(pb => $"Element: {pb.ElementName}");

        return progressBar;
    }

    public override string ToString()
    {
        return ToString(2);
    }
    
    /// <summary>
    /// Converts to string
    /// </summary>
    /// <param name="PRECISION">Precision for decimal results. If set to -1, numbers aren't cropped.</param>
    /// <returns>Matrix of results in string format</returns>
    public string ToString(int PRECISION)
    {
        if (result_matrix ==  null) { return "Warning: No matrix to show!"; }

        string[] lines = new string[bookIDBs.Count + 1];
        List<string> bookIdentification = bookIDBs.Select(x => (x).ToString()).ToList();
        int maxLengthForNumber = PRECISION == 0 ? 1 : PRECISION + 2;
        int maxLengthPerField = Math.Max(bookIdentification.Max(x => x.Length), maxLengthForNumber);
        
        if (PRECISION == -1)
        {
            maxLengthForNumber = 0;
            for (int i = 0; i < bookIDBs.Count; i++)
            {
                for (int j = 0; j < bookIDBs.Count; j++)
                {
                    maxLengthForNumber = Math.Max(maxLengthForNumber, result_matrix[i, j].ToString().Length);
                    maxLengthPerField = Math.Max(maxLengthPerField, maxLengthForNumber);
                }
            }
        }

        lines[0] = " ";
        for (int i = 0; i < maxLengthPerField; i++)
        {
            lines[0] += " ";
        }
        foreach (string bookName in bookIdentification)
        {
            for (int i = bookName.Length; i < maxLengthPerField; i++)
            {
                lines[0] += " ";
            }
            lines[0] += $"{bookName} ";
        }

        for (int idx_line = 1; idx_line < lines.Length; idx_line++)
        {
            string bookName = bookIdentification[idx_line - 1];

            for (int i = bookName.Length; i < maxLengthPerField; i++)
            {
                lines[idx_line] += " ";
            }
            lines[idx_line] += $"{bookName} ";

            
            for (int idx_number = 0; idx_number < bookIDBs.Count; idx_number++)
            {
                string number;
                if (PRECISION == -1)
                {
                    number = result_matrix[idx_line-1, idx_number].ToString();
                } else
                {
                    number = Math.Round(result_matrix[idx_line-1, idx_number], PRECISION).ToString();
                }

                if (number.Length == 1 && number.Length != maxLengthForNumber)
                {
                    number += ",";
                }

                while (number.Length != maxLengthForNumber)
                {
                    number += "0";
                }

                for (int i = number.Length; i < maxLengthPerField; i++)
                {
                    lines[idx_line] += " ";
                }

                lines[idx_line] += $"{number}";
                if (idx_number != bookIDBs.Count - 1)
                {
                    lines[idx_line] += " ";
                }
            }

        }


        return string.Join("\n", lines);
    }

}

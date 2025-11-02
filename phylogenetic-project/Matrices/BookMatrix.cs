using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using iluvadev.ConsoleProgressBar;
using Microsoft.VisualBasic;
using phylogenetic_project.Algorithms;
using phylogenetic_project.Matrices.CellChapterJobs;
using phylogenetic_project.Persistance;
using SQLitePCL;

namespace phylogenetic_project.Matrices;

public class BookMatrix<T_FieldData>
{
    private T_FieldData[,][] matrix;
    decimal[,]? result_matrix;

    public List<int> bookIDBs = new List<int>();
    public List<int> chapters = new List<int>();

    public IMatrixCellChapterJob<T_FieldData>? matrixCellChapterJob;

    public CacheDBIDWrapper? cacheDBIDWrapper;

    public BookMatrix(
        List<int> bookIDBs_,
        List<int> chapters_,
        IMatrixCellChapterJob<T_FieldData>? matrixCellChapterJob_,
        CacheDBIDWrapper? cacheDBIDWrapper_ = null)
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
        cacheDBIDWrapper = cacheDBIDWrapper_;
    }

    public decimal[,]? CalculateResultMatrix(bool showProgressBar = true)
    {
        if (matrixCellChapterJob == null) { return null; }

        using var progressBar = InitProgressBar(showProgressBar);

        result_matrix = new decimal[this.bookIDBs.Count, this.bookIDBs.Count];

        for (int idx_idb1 = 0; idx_idb1 < bookIDBs.Count; idx_idb1++)
        {
            for (int idx_idb2 = idx_idb1 + 1; idx_idb2 < bookIDBs.Count; idx_idb2++)
            {
                for (int idx_chapter = 0; idx_chapter < chapters.Count; idx_chapter++)
                {
                    if (cacheDBIDWrapper != null && cacheDBIDWrapper.cacheDB != null)
                    {
                        string? result = cacheDBIDWrapper.cacheDB.TryToGetFromCache(cacheDBIDWrapper.algorithmName, cacheDBIDWrapper.algorithmArgs, bookIDBs[idx_idb1], bookIDBs[idx_idb2], chapters[idx_chapter]);
                        if (result != null)
                        {
                            T_FieldData? obj = JsonSerializer.Deserialize<T_FieldData>(result);
                            if (obj != null)
                            {
                                matrix[idx_idb1, idx_idb2][idx_chapter] = obj;
                                matrix[idx_idb2, idx_idb1][idx_chapter] = obj;
                                progressBar?.PerformStep(1, $"matrixCellChapterJob.Calculate(idb_{idx_idb1}, idb_{idx_idb2}, chap_{idx_chapter})");
                                continue;
                            }
                        }
                    }
                    matrix[idx_idb1, idx_idb2][idx_chapter] = matrixCellChapterJob.Calculate(idx_idb1, idx_idb2, idx_chapter);
                    matrix[idx_idb2, idx_idb1][idx_chapter] = matrix[idx_idb1, idx_idb2][idx_chapter];

                    cacheDBIDWrapper?.cacheDB?.InsertCache(cacheDBIDWrapper.algorithmName, cacheDBIDWrapper.algorithmArgs, JsonSerializer.Serialize(matrix[idx_idb1, idx_idb2][idx_chapter]), bookIDBs[idx_idb1], bookIDBs[idx_idb2], chapters[idx_chapter]);
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

    public void CalculateResultMatrixInParallel(string jobId, bool showProgressBar = true)
    {
        if (matrixCellChapterJob == null) { return; }
        if (cacheDBIDWrapper == null) {
            throw new ArgumentException("Error: Cache not provided");
        }

        using var progressBar = InitProgressBar(showProgressBar);

        int maxProcesses = Environment.ProcessorCount;


        var tasks = new List<Task>();

        int size = (int)Math.Ceiling((double)chapters.Count / maxProcesses);

        
        foreach (var item in chapters
            .Select((value, index) => new { value, index })
            .GroupBy(x => x.index / size)
            .Select(g => g.Select(v => v.value).ToList())
            .ToList())
        {
            string arguments = $"{jobId} {string.Join(",", bookIDBs.Select(el => el.ToString()))} {string.Join(",", item.Select(el => el.ToString()))}";

            tasks.Add(RunParallelProcess(arguments));
        }

        CreateParallelStatusTask(progressBar);
        Task.WaitAll(tasks.ToArray());

        CalculateResultMatrix(false);
    }

    private Task CreateParallelStatusTask(ProgressBar? progressBar)
    {
        return Task.Run(() =>
        {
            bool[,] doneMatrix = new bool[this.bookIDBs.Count, this.bookIDBs.Count];
            int previousDone = 0;
            while (true)
            {
                int nowDone = 0;
                bool everythingDone = true;
                for (int idx_idb1 = 0; idx_idb1 < bookIDBs.Count; idx_idb1++)
                {
                    for (int idx_idb2 = idx_idb1 + 1; idx_idb2 < bookIDBs.Count; idx_idb2++)
                    {
                        for (int idx_chapter = 0; idx_chapter < chapters.Count; idx_chapter++)
                        {
                            if (cacheDBIDWrapper != null && cacheDBIDWrapper.cacheDB != null)
                            {
                                string? result = cacheDBIDWrapper.cacheDB.TryToGetFromCache(cacheDBIDWrapper.algorithmName, cacheDBIDWrapper.algorithmArgs, bookIDBs[idx_idb1], bookIDBs[idx_idb2], chapters[idx_chapter]);
                                if (result != null)
                                {
                                    T_FieldData? obj = JsonSerializer.Deserialize<T_FieldData>(result);
                                    if (obj != null)
                                    {
                                        doneMatrix[idx_idb1, idx_idb2] = true;
                                        doneMatrix[idx_idb2, idx_idb1] = true;
                                        nowDone++;
                                        everythingDone = false;
                                    }
                                }
                            }
                        }
                    }
                }
                while (previousDone < nowDone)
                {
                    previousDone++;
                    progressBar?.PerformStep(1, $"[PARALLEL] matrixCellChapterJob.Calculate()");             
                }

                if (everythingDone)
                {
                    break;
                }
                Thread.Sleep(5000);
            }

        });
    }

    private Task RunParallelProcess(string arguments)
    {
        return Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project " + Path.Combine(Program.projectPath, "phylogenetic-project") + " " + arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                var proc = Process.Start(psi);
                if (proc == null)
                {
                    throw new ArgumentException("Process didn't start");
                }

                lock (Program.runningProcesses)
                    Program.runningProcesses.Add(proc);

                if (Program.cts.Token.IsCancellationRequested)
                {
                    proc.Kill(entireProcessTree: true);
                    return;
                }

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                lock (Program.runningProcesses)
                    Program.runningProcesses.Remove(proc);

            });
    }

    public ProgressBar? InitProgressBar(bool showProgressBar = true)
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
        if (result_matrix == null) { return "Warning: No matrix to show!"; }

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
                    number = result_matrix[idx_line - 1, idx_number].ToString();
                }
                else
                {
                    number = Math.Round(result_matrix[idx_line - 1, idx_number], PRECISION).ToString();
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


        return string.Join("\n", lines) + '\n';
    }

    public decimal[][] ConvertResultToLowerTriangularMatrix()
    {
        if (result_matrix == null) { return []; }

        int n = bookIDBs.Count;
        decimal[][] newMatrix = new decimal[n][];

        for (int i = 0; i < n; i++)
        {
            newMatrix[i] = new decimal[i + 1];
            for (int j = 0; j <= i; j++)
            {
                newMatrix[i][j] = result_matrix[i, j];
            }
        }

        return newMatrix;
    }
    
}

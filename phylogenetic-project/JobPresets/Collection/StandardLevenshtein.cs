using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.JobPresets.Collection;

public class StandardLevenshtein : IJobPreset
{
    public static string jobId => "StandardLevenshteinAlgorithm";

    public List<int> bookIDBs { get; set; } = new List<int>();
    public List<int> chapters { get; set; } = new List<int>();
    public IGetChapter getChapterConstruct { get; set; } = null!;

    private string timeNow = DateTime.UtcNow.ToString("yyyy.MM.dd_HHmmss");

    public void Start()
    {
        ArgumentNullException.ThrowIfNull(getChapterConstruct);

        var levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinCellChapterJob_FieldData>(
             bookIDBs_: bookIDBs,
             chapters_: chapters,
             matrixCellChapterJob_: new Matrices.CellChapterJobs.LevenshteinCellChapterJob(getChapterConstruct)
         );


        _ = levenshteinMatrix.CalculateResultMatrix();
        Console.WriteLine(levenshteinMatrix.ToString());


        string folderPath = Path.Combine(Program.dataAndResultsPath ?? "", $"results\\temporary\\{timeNow}");
        Directory.CreateDirectory(folderPath);
        File.WriteAllText(Path.Combine(folderPath, "matrix.txt"), levenshteinMatrix.ToString(-1));
        File.WriteAllText(Path.Combine(folderPath, "config.txt"), $"""
            Algorithm used: {jobId}
            
             - chapter text from: {getChapterConstruct.chapterGetterId}
             - bookIDBs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
             - chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            """);

        Console.WriteLine($"All results saved in: \"{folderPath}\" \n");
    }
}

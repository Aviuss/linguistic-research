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
    }
}

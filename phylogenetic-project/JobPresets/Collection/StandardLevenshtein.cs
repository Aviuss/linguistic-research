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

        StaticMethods.SaveTemporaryResults.Save(timeNow, new (string, string)[]
        {
            ("matrix.txt", levenshteinMatrix.ToString(-1)),
            ("config.txt", $"""
            Algorithm used: {jobId}
            
             - chapter text from: {getChapterConstruct.chapterGetterId}
             - bookIDBs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
             - chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            """)
        });

        Console.WriteLine(levenshteinMatrix.ConvertToPythonList());

        StaticMethods.Python.CallPythonScript(
            "create_nj_newick.py",
            new string[] { @"{""save_path"": """", ""inputmatrix"": []}" }
        );
        //StaticMethods.Python.CallPythonScript("create_linguistic_trees.py", new string[] { @"{""testProperty"": ""mleko""}" });
    }
}

internal record struct NewStruct(string Item1, string Item2)
{
    public static implicit operator (string, string)(NewStruct value)
    {
        return (value.Item1, value.Item2);
    }

    public static implicit operator NewStruct((string, string) value)
    {
        return new NewStruct(value.Item1, value.Item2);
    }
}
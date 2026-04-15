using phylogenetic_project.JobPresents;
using phylogenetic_project.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;


namespace phylogenetic_project.JobPresets.Collection;

public class StandardLevenshtein : IJobPreset
{
    public List<int> bookIDBs = null!;
    public List<int> chapters = null!;
    public IGetChapter getChapterConstruct  = null!;

    private string timeNow = DateTime.UtcNow.ToString("yyyy.MM.dd_HHmmss");

    public StandardLevenshtein(IGetChapter getChapterConstruct, List<int> chapters, List<int> bookIDBs)
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
    }

    public void Start()
    {
        ArgumentNullException.ThrowIfNull(getChapterConstruct);

        var levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinIndividualDataInt>(
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
            Algorithm used: {"..."}
            
             - chapter text from: {getChapterConstruct.chapterGetterId}
             - bookIDBs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
             - chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            """)
        });
        
        var pyDataNewick = new
        {
            save_path_newick = Path.Combine(StaticMethods.SaveTemporaryResults.TemporaryFolderPath(timeNow), "newick.txt"),
            inputmatrix = levenshteinMatrix.ConvertResultToLowerTriangularMatrix(),
            names = bookIDBs.Select(element =>
            {
                if (Program.mapIdbToName != null && Program.mapIdbToName.TryGetValue(element, out string? value))
                {
                    if (value != null)
                    {
                        return value;
                    }
                }

                return "idb_" + element.ToString();
            }).ToList()
        };
        StaticMethods.Python.CallPythonScript(
            "create_nj_newick.py",
            new string[] { JsonSerializer.Serialize(pyDataNewick, new JsonSerializerOptions { WriteIndented = true }) }
        );
        
        var pyDataGraph = new
        {
            save_path_graph = Path.Combine(StaticMethods.SaveTemporaryResults.TemporaryFolderPath(timeNow), "graph.png"),
            newickFormat = File.ReadAllText(Path.Combine(StaticMethods.SaveTemporaryResults.TemporaryFolderPath(timeNow), "newick.txt"))
        };
        StaticMethods.Python.CallPythonScript(
            "create_linguistic_trees.py",    
            new string[] { JsonSerializer.Serialize(pyDataGraph, new JsonSerializerOptions { WriteIndented = true }) }
        );
    }
}
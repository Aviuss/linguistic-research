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
    private List<int> bookIDBs = null!;
    private List<int> chapters = null!;
    private IGetChapter getChapterConstruct = null!;
    private string outputResultPath = null!;
    private bool noPython = false;

    public StandardLevenshtein(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        string outputResultPath,
        bool noPython = false
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.outputResultPath = outputResultPath;
        this.noPython = noPython;
    }

    public void Start()
    {
        var levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinIndividualDataInt>(
             bookIDBs_: bookIDBs,
             chapters_: chapters,
             matrixCellChapterJob_: new Matrices.CellChapterJobs.LevenshteinCellChapterJob(getChapterConstruct)
         );

        _ = levenshteinMatrix.CalculateResultMatrix();
        Console.WriteLine(levenshteinMatrix.ToString());


        StaticMethods.SaveTemporaryResults.Save(
            this.outputResultPath,
            [
                ("matrix.txt", levenshteinMatrix.ToString(-1)),
                ("config.txt", $"""
                --job phylogenetic-tree-standard-text
                
                --input-type-id {getChapterConstruct.resourceId}
                --book-idbs {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
                --chapters {string.Join(", ", chapters.Select(chap => chap.ToString()))}
                """)
            ]
        );
        
        var pyDataNewick = new
        {
            save_path_newick = Path.Combine(this.outputResultPath, "newick.txt"),
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

        if (!noPython)
        {        
            StaticMethods.Python.CallPythonScript(
                "create_nj_newick.py",
                new string[] { JsonSerializer.Serialize(pyDataNewick, new JsonSerializerOptions { WriteIndented = true }) }
            );
            
            var pyDataGraph = new
            {
                save_path_graph = Path.Combine(this.outputResultPath, "graph.png"),
                newickFormat = File.ReadAllText(Path.Combine(this.outputResultPath, "newick.txt"))
            };
            StaticMethods.Python.CallPythonScript(
                "create_linguistic_trees.py",    
                new string[] { JsonSerializer.Serialize(pyDataGraph, new JsonSerializerOptions { WriteIndented = true }) }
            );   
        }
    }
}
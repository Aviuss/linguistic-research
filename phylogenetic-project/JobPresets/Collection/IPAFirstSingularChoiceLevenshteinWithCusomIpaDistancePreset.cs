
using phylogenetic_project.JobPresets;
using phylogenetic_project.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Concurrent;


namespace phylogenetic_project.JobPresets.Collection;

public class IPAFirstSingularChoiceLevenshteinWithCusomIpaDistancePreset : IJobPreset
{
    public List<int> bookIDBs { get; set; } = new List<int>();
    public List<int> chapters { get; set; } = new List<int>();
    public IGetChapter getChapterConstruct { get; set; } = null!;
    private string outputResultPath = null!;
    private bool noPython = false;
    private ConcurrentDictionary<int, string>? mapIdbToName = null;
    private LanguageRules[] listOfLanguageRules = null!;
    private IpaCustomLetterDistance ipaLetterDistanceDict = null!;
    // /int workers = 1; // 1 -> single threaded, >1 -> parallel

    public IPAFirstSingularChoiceLevenshteinWithCusomIpaDistancePreset(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        string outputResultPath,
        Persistance.LanguageRules[] listOfLanguageRules,
        IpaCustomLetterDistance ipaLetterDistanceDict,
        bool noPython = false,
        ConcurrentDictionary<int, string>? mapIdbToName = null
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.outputResultPath = outputResultPath;
        this.listOfLanguageRules = listOfLanguageRules;
        this.noPython = noPython;
        this.mapIdbToName = mapIdbToName;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
    }

    public void Start()
    {
        var levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinIndividualDataDecimal>(
            bookIDBs_: bookIDBs,
            chapters_: chapters,
            matrixCellChapterJob_: new Matrices.CellChapterJobs.IPAFirstSingularChoiceLevenshteinCellChapterJobWithCustomIpaDistance(
                getChapterConstruct: getChapterConstruct,
                ipaDistanceElement_: this.ipaLetterDistanceDict,
                listOfLanguageRules: this.listOfLanguageRules
            ),
            cacheDBIDWrapper_: null //new Persistance.CacheDBIDWrapper(Program.cacheDB, "aaaa TODO here", "")
         );

        _ = levenshteinMatrix.CalculateResultMatrix();
        //else if (workers > 1)
        //  levenshteinMatrix.CalculateResultMatrixInParallel("TODO AAAAA");
        

        Console.WriteLine(levenshteinMatrix.ToString());

        StaticMethods.SaveTemporaryResults.Save(this.outputResultPath, new (string, string)[]
        {
            ("matrix.txt", levenshteinMatrix.ToString(-1)),
            ("config.txt", $"""
            job: phylogenetic-tree-ipa-singular-choice w custom-ipa-distance
            
            input-type-id: {getChapterConstruct.resourceId}
            book-idbs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
            chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            """)
        });
        
        if (!noPython)
        {
            var pyDataNewick = new
            {
                save_path_newick = Path.Combine(this.outputResultPath, "newick.txt"),
                inputmatrix = levenshteinMatrix.ConvertResultToLowerTriangularMatrix(),
                names = bookIDBs.Select(element =>
                {
                    if (this.mapIdbToName != null && this.mapIdbToName.TryGetValue(element, out string? value))
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
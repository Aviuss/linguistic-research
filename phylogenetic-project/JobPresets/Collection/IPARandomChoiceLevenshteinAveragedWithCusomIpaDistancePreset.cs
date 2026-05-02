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

public class IPARandomChoiceLevenshteinAveragedWithCusomIpaDistancePreset : IJobPreset
{
    private List<int> bookIDBs { get; set; } = new List<int>();
    private List<int> chapters { get; set; } = new List<int>();
    public IGetChapter getChapterConstruct { get; set; } = null!;
    private string outputResultPath = null!;
    private bool noPython = false;
    private ConcurrentDictionary<int, string>? mapIdbToName = null;
    private LanguageRulesWrapper languageRulesWrapper = null!;
    private IpaCustomLetterDistance ipaLetterDistanceDict = null!;
    // /int workers = 1; // 1 -> single threaded, >1 -> parallel
    private int randomSize = 10;
    private CacheDB? cachedb = null;

    public IPARandomChoiceLevenshteinAveragedWithCusomIpaDistancePreset(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        string outputResultPath,
        Persistance.LanguageRulesWrapper languageRulesWrapper,
        IpaCustomLetterDistance ipaLetterDistanceDict,
        int randomSize,
        bool noPython = false,
        ConcurrentDictionary<int, string>? mapIdbToName = null,
        CacheDB? cachedb = null
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.outputResultPath = outputResultPath;
        this.languageRulesWrapper = languageRulesWrapper;
        this.noPython = noPython;
        this.mapIdbToName = mapIdbToName;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
        this.randomSize = randomSize;
        this.cachedb = cachedb;
    }

    public void Start()
    {
        Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinIndividualDataDecimal> levenshteinMatrix;

        levenshteinMatrix = new Matrices.BookMatrix<Matrices.CellChapterJobs.LevenshteinIndividualDataDecimal>(
            bookIDBs_: bookIDBs,
            chapters_: chapters,
            matrixCellChapterJob_: new Matrices.CellChapterJobs.IPARandomChoiceLevenshteinCellChapterJobWithCustomIpaDistance(
                getChapterConstruct: getChapterConstruct,
                randomSize_: this.randomSize,
                ipaDistanceElement_: this.ipaLetterDistanceDict,
                parallelExecution_: 1,
                listOfLanguageRules: this.languageRulesWrapper.languageRules
            ),
            cacheDBIDWrapper_: new Persistance.CacheDBIDWrapper(
                this.cachedb,
                "phylogenetic-tree-ipa-random-choice w custom-ipa-distance",
                $"""randomSize: {this.randomSize}; input-id: {this.getChapterConstruct.resourceId}; ipa-rules-id: {languageRulesWrapper.resourceId}; custom-ipa-distance: {this.ipaLetterDistanceDict.resourceId}"""
            )
        );

        _ = levenshteinMatrix.CalculateResultMatrix();

        Console.WriteLine(levenshteinMatrix.ToString());

        StaticMethods.SaveTemporaryResults.Save(this.outputResultPath, new (string, string)[]
        {
            ("matrix.txt", levenshteinMatrix.ToString(-1)),
            ("config.txt", $"""
            job: phylogenetic-tree-ipa-random-choice w custom-ipa-distance
            
            input-id: {getChapterConstruct.resourceId}
            ipa-rules-id: {languageRulesWrapper.resourceId}
            custom-ipa-distance: {this.ipaLetterDistanceDict.resourceId}
            book-idbs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
            chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            random-size: {this.randomSize}
            """)
        });
        
        if (!this.noPython)
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
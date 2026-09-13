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

public class AnalyzeMissingLettersFromIpaRules : IJobPreset
{
    private List<int> bookIDBs { get; set; } = new List<int>();
    private List<int> chapters { get; set; } = new List<int>();
    public IGetChapter getChapterConstruct { get; set; } = null!;
    private string outputResultPath = null!;
    private ConcurrentDictionary<int, string>? mapIdbToName = null;
    private LanguageRulesWrapper languageRulesWrapper = null!;
    private IpaCustomLetterDistance? ipaLetterDistanceDict = null;

    public AnalyzeMissingLettersFromIpaRules(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        string outputResultPath,
        Persistance.LanguageRulesWrapper languageRulesWrapper,
        IpaCustomLetterDistance? ipaLetterDistanceDict,
        ConcurrentDictionary<int, string>? mapIdbToName = null
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.outputResultPath = outputResultPath;
        this.languageRulesWrapper = languageRulesWrapper;
        this.mapIdbToName = mapIdbToName;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
    }

    public void Start()
    {
        StringBuilder resultsForTextMissingInIpaRules = new();
        foreach (int bookIDB in bookIDBs)
        {
            resultsForTextMissingInIpaRules.Append(EvaulateBookForTextMissingInIpaRules(bookIDB));
        }        


        StaticMethods.SaveTemporaryResults.Save(this.outputResultPath, new (string, string)[]
        {
            ("config.txt", $"""
            job: analyze-missing-letters-from-ipa-rules
            
            input-id: {getChapterConstruct.resourceId}
            ipa-rules-id: {languageRulesWrapper.resourceId}
            custom-ipa-distance: {this.ipaLetterDistanceDict?.resourceId ?? "not defined"}
            book-idbs: {string.Join(", ", bookIDBs.Select(idb => idb.ToString()))}
            chapters: {string.Join(", ", chapters.Select(chap => chap.ToString()))}
            """),
            ("results.txt", $"""
            aaa
            """)
        });
    }

    private StringBuilder EvaulateBookForTextMissingInIpaRules(int bookIDB)
    {
        

        return new();
    }

}
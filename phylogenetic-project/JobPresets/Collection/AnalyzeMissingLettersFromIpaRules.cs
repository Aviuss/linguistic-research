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

    private HashSet<string> textLetterBlackList = null!;

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
        this.textLetterBlackList =  new() {"-", ",", ":", "!", "?", ".", "…", "“", "”", "„", "(", ")", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "–", "—", ";", "«", "»", "\"", "‑", " ", " ", "­", "̈"};

    }

    public void Start()
    {
        StaticMethods.ConsoleProgress.Start();
        StaticMethods.ConsoleProgress.total = bookIDBs.Count*chapters.Count;


        StringBuilder resultsForTextMissingInIpaRules = new();
        foreach (int bookIDB in this.bookIDBs)
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
            ("results.txt", resultsForTextMissingInIpaRules.ToString())
        });
    }

    private StringBuilder EvaulateBookForTextMissingInIpaRules(int bookIDB)
    {
        Persistance.LanguageRules? ipaRule = Array.Find(this.languageRulesWrapper.languageRules, element => element.IdbCompatible.Contains(bookIDB));
        ArgumentNullException.ThrowIfNull(ipaRule);

        SortedSet<string> lettersMissingInIpaRules = new();
        foreach (var chapter in this.chapters)
        {
            string chapterText = this.getChapterConstruct.GetChapter(bookIDB, chapter);
            lettersMissingInIpaRules.UnionWith(
                phylogenetic_project.StaticMethods.IPA.ConvertToIpa_ReturnLettersWhichDontConvert(chapterText, ipaRule)
            );

            StaticMethods.ConsoleProgress.PerformStep(1, $"ConvertToIpa_ReturnLettersWhichDontConvert()");
        }
        
        lettersMissingInIpaRules.ExceptWith(this.textLetterBlackList);

        StringBuilder results = new();
        if (lettersMissingInIpaRules.Count == 0)
        {
            return results;
        }

        results.Append(
            string.Format(
                "book '{0}' has {1} unmatched symbols in rules for text to ipa conversion:\n",
                getBookName(bookIDB), lettersMissingInIpaRules.Count
            )
        );

        foreach (var x in lettersMissingInIpaRules) {
            results.Append(string.Format("{0}\n", x));
        }
        
        results.Append("\n");

        return results;
    }

    private string getBookName(int bookIDB)
    {
        if (this.mapIdbToName != null && this.mapIdbToName.TryGetValue(bookIDB, out string? value))
        {
            if (value != null)
            {
                return value;
            }
        }

        return "idb_" + bookIDB.ToString();
    }

}
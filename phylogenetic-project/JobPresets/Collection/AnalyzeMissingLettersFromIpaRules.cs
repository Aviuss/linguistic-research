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
        StaticMethods.ConsoleProgress.Start();
        StaticMethods.ConsoleProgress.total = 
            bookIDBs.Count*chapters.Count + 
            (ipaLetterDistanceDict != null ? bookIDBs.Count : 0);

        List<(int bookIDB, SortedSet<string> missing)> lettersMissingPerBook = new();
        foreach (int bookIDB in this.bookIDBs)
        {
            lettersMissingPerBook.Add((bookIDB, EvaulateBookForTextMissingInIpaRules(bookIDB)));
        }
        StringBuilder resultsForTextMissingInIpaRules = FormatTextMissingInIpaRules(lettersMissingPerBook);

        StringBuilder resultsIpaRulesCoverage = new();
        if (ipaLetterDistanceDict != null)
        {
            foreach (int bookIDB in this.bookIDBs)
            {
                resultsIpaRulesCoverage.Append(EvaulateIpaRulesCoverage(bookIDB));
                StaticMethods.ConsoleProgress.PerformStep(1, $"EvaulateIpaRulesCoverage()");
            }    
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
            ("resultsForTextMissingInIpaRules.txt", resultsForTextMissingInIpaRules.ToString()),
            ("resultsIpaRulesCoverage.txt", resultsIpaRulesCoverage.ToString())
        });
    }

    private SortedSet<string> EvaulateBookForTextMissingInIpaRules(int bookIDB)
    {
        Persistance.LanguageRules? ipaRule = Array.Find(this.languageRulesWrapper.languageRules, element => element.IdbCompatible.Contains(bookIDB));
        ArgumentNullException.ThrowIfNull(ipaRule);

        SortedSet<string> lettersMissingInIpaRules = new(StringComparer.Ordinal);
        foreach (var chapter in this.chapters)
        {
            string chapterText = this.getChapterConstruct.GetChapter(bookIDB, chapter);
            lettersMissingInIpaRules.UnionWith(
                phylogenetic_project.StaticMethods.IPA.ConvertToIpa_ReturnLettersWhichDontConvert(chapterText, ipaRule)
            );
            StaticMethods.ConsoleProgress.PerformStep(1, $"ConvertToIpa_ReturnLettersWhichDontConvert()");
        }

        return lettersMissingInIpaRules;
    }

    private StringBuilder FormatTextMissingInIpaRules(List<(int bookIDB, SortedSet<string> missing)> lettersMissingPerBook)
    {
        StringBuilder results = new();
        var booksWithMissing = lettersMissingPerBook.Where(x => x.missing.Count > 0).ToList();
        if (booksWithMissing.Count == 0)
        {
            return results;
        }

        // symbols missing in every book that has any missing symbols; printed once instead of per book
        SortedSet<string> common = new(StringComparer.Ordinal);
        if (booksWithMissing.Count > 1)
        {
            common.UnionWith(booksWithMissing[0].missing);
            foreach (var (_, missing) in booksWithMissing.Skip(1))
            {
                common.IntersectWith(missing);
            }
        }

        if (common.Count > 0)
        {
            results.Append(
                string.Format(
                    "{0} unmatched symbols in rules for text to ipa conversion common to all {1} books listed below:\n",
                    common.Count, booksWithMissing.Count
                )
            );
            AppendSymbols(results, common);
            results.Append("\n");
        }

        foreach (var (bookIDB, missing) in booksWithMissing)
        {
            var specific = missing.Where(x => !common.Contains(x)).ToList();
            results.Append(
                string.Format(
                    "book '{0}' has {1} unmatched symbols in rules for text to ipa conversion{2}\n",
                    getBookName(bookIDB), missing.Count,
                    common.Count == 0 ? ":"
                        : specific.Count == 0 ? $" (only the {common.Count} common ones)"
                        : $" ({common.Count} common + {specific.Count} specific):"
                )
            );
            AppendSymbols(results, specific);
            results.Append("\n");
        }

        return results;
    }

    private static void AppendSymbols(StringBuilder results, IEnumerable<string> symbols)
    {
        foreach (var x in symbols) {
            results.Append(string.Format("{0}\t[{1}]\n", x, string.Join(" ", x.Select(c => $"U+{(int)c:X4}"))));
        }
    }

    private StringBuilder EvaulateIpaRulesCoverage(int bookIDB) {
        ArgumentNullException.ThrowIfNull(this.ipaLetterDistanceDict);
        Persistance.LanguageRules? ipaRule = Array.Find(this.languageRulesWrapper.languageRules, element => element.IdbCompatible.Contains(bookIDB));
        ArgumentNullException.ThrowIfNull(ipaRule);


        HashSet<string> ipaDestinationRules = ipaRule.Rules
            .Select(el => el.Value)
            .Where(x => x != null)
            .SelectMany(x => x)
            .Where(x => x != null)
            .Select(x => x)
            .Select(x => Algorithms.LevenshteinCustomIpaDistance.EnumerateVisualIPAText(x).ToArray())
            .SelectMany(x => x)
            .Select(x => x.Item1)
            .ToArray()
            .ToHashSet();

        List<string> missingIpaDistances = [];
        foreach (string ipa in ipaDestinationRules)
        {
            if (!this.ipaLetterDistanceDict.HasString(ipa))
            {
                missingIpaDistances.Add(ipa);
            }
        }
        missingIpaDistances.Sort();

        StringBuilder results = new();
        if (missingIpaDistances.Count == 0)
        {
            return results;
        }

        results.Append(
            string.Format(
                "book '{0}' has {1} unmatched ipa symbols in the distance rules:\n",
                getBookName(bookIDB), missingIpaDistances.Count
            )
        );

        foreach (var x in missingIpaDistances) {
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
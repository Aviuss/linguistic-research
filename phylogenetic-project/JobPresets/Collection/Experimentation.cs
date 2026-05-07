using System;
using phylogenetic_project.JobPresets;
using phylogenetic_project.Persistance;

namespace phylogenetic_project.JobPresets.Collection;

public class Experimentation: IJobPreset
{
    private IGetChapter getChapterConstruct;
    private List<int> chapters;
    private List<int> bookIDBs;
    private Persistance.LanguageRulesWrapper languageRulesWrapper;
    private IpaCustomLetterDistance ipaLetterDistanceDict;
    private decimal threshold; //values should be >= than this
    private int randomIpaSize = 10;

    record Element(string text, int idb);

    private List<List<Element>> listOfCliques = [];


    public Experimentation(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        Persistance.LanguageRulesWrapper languageRulesWrapper,
        IpaCustomLetterDistance ipaLetterDistanceDict,
        decimal threshold,
        int randomIpaSize = 10
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.languageRulesWrapper = languageRulesWrapper;
        this.threshold = threshold;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
        this.randomIpaSize = randomIpaSize;
    }

    public void Start()
    {
        Console.WriteLine("Experiment!");
        
        for (int idx_chapter = 0; idx_chapter < chapters.Count; idx_chapter++)
        {
            processChapter(chapters[idx_chapter]);                
        }

        this.listOfCliques = this.listOfCliques
            .Distinct()
            .OrderByDescending(e => e.Count)
            .Where(e => e.Count > 9) // for less results | ONLY TEST
            .ToList();

        foreach (var clique in this.listOfCliques)
        {
            Console.WriteLine($" [{string.Join(", ", clique)}]");    
        }


    }

    

    private void processChapter(int chapter)
    {
        List<Element> elements = [];
        for (int idx_idb = 0; idx_idb < this.bookIDBs.Count; idx_idb++)
        {
            int idb1 = this.bookIDBs[idx_idb];
            
            elements.AddRange(
                getChapterConstruct.GetChapter(idb1, chapter)
                    .ToLowerInvariant()
                    .Split(null)
                    .Where(e => e.Length > 0)
                    .Distinct()
                    .Select(e => new Element(e, idb1))
            ); 
        }
        
        if (elements.Count == 0)
            return;

        
        var finder = new phylogenetic_project.Algorithms.CliqueFinder<Element>();

        for (int i1 = 0; i1 < elements.Count; i1++)
        {
            for (int i2 = i1+1; i2 < elements.Count; i2++)
            {
                Element e1 = elements[i1];
                Element e2 = elements[i2];

                Persistance.LanguageRules? ipaRule_idb1 = Array.Find(this.languageRulesWrapper.languageRules, element => element.IdbCompatible.Contains(e1.idb));
                ArgumentNullException.ThrowIfNull(ipaRule_idb1);
                Persistance.LanguageRules? ipaRule_idb2 = Array.Find(this.languageRulesWrapper.languageRules, element => element.IdbCompatible.Contains(e2.idb));
                ArgumentNullException.ThrowIfNull(ipaRule_idb2);


                var ipaText_idb1 = StaticMethods.IPA.ConvertToIpa(
                    e1.text,
                    ipaRule_idb1
                );

                var ipaText_idb2 = StaticMethods.IPA.ConvertToIpa(
                    e2.text,
                    ipaRule_idb2
                );

                var res = Algorithms.LevenshteinIPARandomChoiceAveragedWithCustomIpaDistance.Calculate(
                    ipaText_idb1,
                    ipaText_idb2,
                    this.ipaLetterDistanceDict,
                    this.randomIpaSize
                );    
                
                var similarity = 1 - (res.levensthein_distance / res.max_chapter_length);

                if (similarity >= threshold)
                {
                    finder.Add((e1, e2));
                }

            }
        }

        foreach (var clique in finder.FindAllCliques())
        {
            int distinct = clique.Select(e => e.idb).Distinct().Count();
            if (distinct > 1)
            {
                listOfCliques.Add(clique.ToList());
            }
        }
            


    }
}

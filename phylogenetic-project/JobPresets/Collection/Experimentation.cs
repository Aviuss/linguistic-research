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
    private int parallelWorkers = 16;

    record Element(string text, int idb);


    public Experimentation(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        Persistance.LanguageRulesWrapper languageRulesWrapper,
        IpaCustomLetterDistance ipaLetterDistanceDict,
        decimal threshold,
        int randomIpaSize = 10,
        int parallelWorkers = 16
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.languageRulesWrapper = languageRulesWrapper;
        this.threshold = threshold;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
        this.randomIpaSize = randomIpaSize;
        this.parallelWorkers = parallelWorkers;
    }

    public void Start()
    {
        Console.WriteLine("Experiment!");
        
        object locker = new object();
        List<List<Element>> listOfCliques = [];

        Parallel.ForEach(
            Enumerable.Range(0, chapters.Count),
            new ParallelOptions { MaxDegreeOfParallelism = this.parallelWorkers },
            idx_chapter =>
            {
                var chapterClique = GetChapterCliques(chapters[idx_chapter]);
                lock (locker)
                {
                    listOfCliques.AddRange(chapterClique);
                }
            }
        );


        listOfCliques = listOfCliques.Distinct(new ListElementComparer()).ToList();
        
        listOfCliques = RemoveSubsets(listOfCliques).ToList();
        
        int preLen;
        int postLen;

        do
        {
            preLen = listOfCliques.Count;
            MergeChapterCliques(listOfCliques);
            listOfCliques = RemoveSubsets(listOfCliques).ToList();
            postLen = listOfCliques.Count;
        } while (postLen < preLen);


        foreach (var clique in listOfCliques.OrderByDescending(e => e.Count))
        {
            Console.WriteLine($" [{string.Join(", ", clique)}]");    
        }
    }
    
    private void MergeChapterCliques(List<List<Element>> listOfCliques)
    {
        List<bool> validMask = Enumerable.Repeat(true, listOfCliques.Count).ToList();

        for (int i = 0; i < listOfCliques.Count; i++)
        {
            if (validMask[i] == false)
                continue;

            for (int j = 0; j < listOfCliques.Count; j++)
            {
                if (validMask[i] == false)
                    continue;
                if (validMask[j] == false)
                    continue;
                if (i == j)
                    continue;

                var clique1 = listOfCliques[i];
                var clique2 = listOfCliques[j];

                var finder = new Algorithms.CliqueFinder<Element>(new ElementComparer());
                
                foreach (Element c1 in clique1)
                {
                    foreach (Element c2 in clique2)
                    {
                        if (c1 == c2)
                            continue;

                        var sim = CalculateSimilarity(c1, c2);
                        if (sim >= threshold)
                        {
                            finder.Add((c1, c2));
                        }
                    }
                }

                if (finder.GetGraphCount() > 0)
                {
                    for (int ii = 0; ii < clique1.Count; ii++)
                    {
                        for (int jj = 0; jj < clique1.Count; jj++)
                        {
                            if (ii != jj)
                            {
                                finder.Add((clique1[ii], clique1[jj]));
                            }
                        }   
                    }
                    
                    for (int ii = 0; ii < clique2.Count; ii++)
                    {
                        for (int jj = 0; jj < clique2.Count; jj++)
                        {
                            if (ii != jj)
                            {
                                finder.Add((clique2[ii], clique2[jj]));
                            }
                        }   
                    }


                    validMask[i] = false;
                    validMask[j] = false;
                    foreach (var clique in finder.FindAllCliques())
                    {
                        int distinct = clique.Select(e => e.idb).Distinct().Count();
                        if (distinct > 1)
                        {
                            listOfCliques.Add(clique.ToList());
                            validMask.Add(false);
                        }
                    }

                }

            }
        }
    }

    private List<List<Element>> GetChapterCliques(int chapter)
    {
        List<List<Element>> listOfCliques = [];
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
            return [];

        
        var finder = new Algorithms.CliqueFinder<Element>(new ElementComparer());

        for (int i1 = 0; i1 < elements.Count; i1++)
        {
            for (int i2 = i1+1; i2 < elements.Count; i2++)
            {
                Element e1 = elements[i1];
                Element e2 = elements[i2];
                
                var similarity = CalculateSimilarity(e1, e2);

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

        return listOfCliques;
    }

    private decimal CalculateSimilarity(Element e1, Element e2)
    {
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
        
        return 1 - (res.levensthein_distance / res.max_chapter_length);
    }

    private static List<List<Element>> RemoveSubsets(List<List<Element>> lists)
    {
        var sets = lists
            .Select(l => (Original: l, Set: new HashSet<Element>(l)))
            .ToList();

        return sets
            .Where(candidate => !sets.Any(other =>
                other.Set.Count > candidate.Set.Count &&
                candidate.Set.IsSubsetOf(other.Set)))
            .Select(x => x.Original)
            .ToList();
    }

    private class ListElementComparer : IEqualityComparer<List<Element>>
    {
        public bool Equals(List<Element>? x, List<Element>? y) =>
            x != null && y != null &&
            x.OrderBy(e => e.idb).ThenBy(agent => agent.text).SequenceEqual(
                y.OrderBy(e => e.idb).ThenBy(agent => agent.text)
            );

        public int GetHashCode(List<Element> obj) =>
            obj.OrderBy(e => e.idb).ThenBy(agent => agent.text)
            .Aggregate(0, (hash, e) => HashCode.Combine(hash, e.GetHashCode()));

    }

    private class ElementComparer : IComparer<Element>
    {
        public int Compare(Element? x, Element? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int cmp = x.idb.CompareTo(y.idb);
            return cmp != 0 ? cmp : string.Compare(x.text, y.text, StringComparison.Ordinal);
        }
    }

}

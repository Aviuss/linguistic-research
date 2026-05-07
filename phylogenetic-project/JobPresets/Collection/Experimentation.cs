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
    public Experimentation(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        Persistance.LanguageRulesWrapper languageRulesWrapper,
        IpaCustomLetterDistance ipaLetterDistanceDict,
        decimal threshold
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.languageRulesWrapper = languageRulesWrapper;
        this.threshold = threshold;
        this.ipaLetterDistanceDict = ipaLetterDistanceDict;
    }

    public void Start()
    {
        Console.WriteLine("Experiment!");

        for (int idx_idb1 = 0; idx_idb1 < bookIDBs.Count; idx_idb1++)
        {
            for (int idx_idb2 = idx_idb1 + 1; idx_idb2 < bookIDBs.Count; idx_idb2++)
            {
                for (int idx_chapter = 0; idx_chapter < chapters.Count; idx_chapter++)
                {
                    processChapter(bookIDBs[idx_idb1], bookIDBs[idx_idb2], chapters[idx_chapter]);                
                    Console.WriteLine("Early return after processChapter iteration!");
                    return;
                }
            }
        }


    }

    private (int, int)? getHighestValue(decimal[,] similarityMatrix)
    {
        (int, int)? highestCoords = null;
        decimal currentHighest = 0;

        for (int x = 0; x < similarityMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < similarityMatrix.GetLength(1); y++)
            {
                if (currentHighest < similarityMatrix[x, y])
                {
                    highestCoords = (x, y);
                    currentHighest = similarityMatrix[x, y];
                    if (currentHighest == 1)
                    {
                        goto done;
                    }
                }
            }
        }
        done:;
        return highestCoords;
    }

    private void processChapter(int idb1, int idb2, int chapter)
    {
        string[] textSplitted1 = getChapterConstruct.GetChapter(idb1, chapter).Split(null).Distinct().ToArray();
        string[] textSplitted2 = getChapterConstruct.GetChapter(idb2, chapter).Split(null).Distinct().ToArray();
        
        long totalLength = textSplitted1.Length + textSplitted2.Length;

        var similarityMatrix = new decimal[totalLength, totalLength];
        // 0  -> most different
        // 1  -> most similar (same)
        // -1 -> used
        

        var finder = new phylogenetic_project.Algorithms.CliqueFinder();
        
        for (int i1 = 0; i1 < totalLength; i1++)
        {
            for (int i2 = i1+1; i2 < totalLength; i2++)
            {
                string text_1;
                if (i1 < textSplitted1.Length)
                {
                    text_1 = textSplitted1[i1];
                } else
                {
                    text_1 = textSplitted2[i1-textSplitted1.Length];
                }
                
                string text_2;
                if (i2 < textSplitted1.Length)
                {
                    text_2 = textSplitted1[i2];
                } else
                {
                    text_2 = textSplitted2[i2-textSplitted1.Length];
                }



                var value = 1 - (decimal)Algorithms.Levenshtein.Distance(text_1, text_2) / Math.Max(text_1.Length, text_2.Length);
                similarityMatrix[i1, i2] = value;
                similarityMatrix[i2, i1] = value;

                Console.WriteLine($"'{text_1}', '{text_2}', {value}");

                if (value >= threshold)
                {
                    finder.AddEdge(i1, i2);
                }

            }
        }

        foreach (var clique in finder.FindAllCliques())
            Console.WriteLine($"  [{string.Join(", ", clique)}]");

        List<Group> gr = new();

    }

    private struct Group
    {
        public Group()
        {
            
        }

        List<GroupMember<string>> groupMembers = new();
    }

    private struct GroupMember<T>
    {
        public GroupMember(T value, int idb, int chapter)
        {
            this.value = value;
            this.idb = idb;
            this.chapter = chapter;
        }

        public int idb;
        public int chapter;
        public T value;
    }
}

using System;
using phylogenetic_project.JobPresets;
using phylogenetic_project.Persistance;

namespace phylogenetic_project.JobPresets.Collection;

public class Experimentation: IJobPreset
{
    private IGetChapter getChapterConstruct;
    private List<int> chapters;
    private List<int> bookIDBs;
    private Persistance.LanguageRules[] listOfLanguageRules;
    private decimal threshold; //values should be >= than this
    public Experimentation(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs,
        Persistance.LanguageRules[] listOfLanguageRules,
        decimal threshold
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
        this.listOfLanguageRules = listOfLanguageRules;
        this.threshold = threshold;
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
        
        var similarityMatrix = new decimal[textSplitted1.Length, textSplitted2.Length];
        // 0  -> most different
        // 1  -> most similar (same)
        // -1 -> used
        
        for (int idx_txt_1 = 0; idx_txt_1 < textSplitted1.Length; idx_txt_1++)
        {
            for (int idx_txt_2 = 0; idx_txt_2 < textSplitted2.Length; idx_txt_2++)
            {
                var a = textSplitted1[idx_txt_1];
                var b = textSplitted2[idx_txt_2];
                var value = 1 - (decimal)Algorithms.Levenshtein.Distance(a, b) / Math.Max(a.Length, b.Length);
                similarityMatrix[idx_txt_1, idx_txt_2] = value;
            }
        }


        List<Group> gr = new();

        do
        {
            (int, int)? highestCoords = getHighestValue(similarityMatrix);
            Console.WriteLine("aa");
        } while (true);

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

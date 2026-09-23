using System;
using phylogenetic_project.StaticMethods;

namespace phylogenetic_project.Persistance;

public class CacheDBIDWrapper
{
    public string algorithmName = "";
    public string algorithmArgs = "";

    public CacheDB? cacheDB;

    private string inputFilesHash;
    private Dictionary<(int idb, int chapter), string> chapterHashes = [];

    // Chapter texts are hashed up front (after normalization, i.e. exactly what the algorithm sees),
    // so the lookup stays thread-safe and does not hit the input database again.
    public CacheDBIDWrapper(CacheDB cacheDB_, string algorithmName_, string algorithmArgs_,
        string inputFilesHash_, IGetChapter getChapter, List<int> bookIDBs, List<int> chapters)
    {
        cacheDB = cacheDB_;
        algorithmName = algorithmName_;
        algorithmArgs = algorithmArgs_;
        inputFilesHash = inputFilesHash_;

        foreach (int idb in bookIDBs)
        {
            foreach (int chapter in chapters)
            {
                chapterHashes[(idb, chapter)] = Hashing.Sha256Hex(getChapter.GetChapter(idb, chapter));
            }
        }
    }

    public string TotalHash(int idb1, int idb2, int chapter)
    {
        return Hashing.Combine(
            ("algorithmVersion", Program.AlgorithmVersion),
            ("inputFiles", inputFilesHash),
            ("text1", chapterHashes[(idb1, chapter)]),
            ("text2", chapterHashes[(idb2, chapter)])
        );
    }
}

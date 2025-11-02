using System;

namespace phylogenetic_project.Persistance;

public class CacheDBIDWrapper
{
    public string algorithmName = "";
    public string algorithmArgs = "";

    public CacheDB? cacheDB;

    public CacheDBIDWrapper(CacheDB? cacheDB_, string algorithmName_, string algorithmArgs_)
    {
        cacheDB = cacheDB_;
        algorithmName = algorithmName_;
        algorithmArgs = algorithmArgs_;
    }
}

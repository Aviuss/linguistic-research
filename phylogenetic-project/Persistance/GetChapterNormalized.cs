using System;
using System.Collections.Concurrent;

namespace phylogenetic_project.Persistance;

public class GetChapterNormalized : IGetChapter
{
    public string resourceId => $"{inner.resourceId}+normalization:{normalizationRules.resourceId}";

    public IGetChapter inner;
    private NormalizationRules normalizationRules;
    private ConcurrentDictionary<(int, int), string> normalizedChapters = new();

    public GetChapterNormalized(IGetChapter inner, NormalizationRules normalizationRules)
    {
        this.inner = inner;
        this.normalizationRules = normalizationRules;
    }

    public string GetChapter(int bookIDB, int chapterNo)
    {
        return normalizedChapters.GetOrAdd(
            (bookIDB, chapterNo),
            key => normalizationRules.Apply(inner.GetChapter(key.Item1, key.Item2))
        );
    }
}

using System;
using phylogenetic_project.JobPresets;
using phylogenetic_project.Persistance;

namespace phylogenetic_project.JobPresets.Collection;

public class Experimentation: IJobPreset
{
    private IGetChapter getChapterConstruct;
    private List<int> chapters;
    private List<int> bookIDBs;

    public Experimentation(
        IGetChapter getChapterConstruct,
        List<int> chapters,
        List<int> bookIDBs
    )
    {
        this.getChapterConstruct = getChapterConstruct;
        this.chapters = chapters;
        this.bookIDBs = bookIDBs;
    }

    public void Start()
    {
        Console.WriteLine("Experiment!");
    }
}

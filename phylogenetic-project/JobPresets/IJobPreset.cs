using phylogenetic_project.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.JobPresents;

public interface IJobPreset
{   
    public void Start();
    public static abstract string jobId { get; }
    public List<int> bookIDBs { get; set; }
    public List<int> chapters { get; set; }
    IGetChapter getChapterConstruct { get; set; }
}

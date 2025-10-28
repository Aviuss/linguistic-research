using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.Persistance;

public interface IGetChapter
{
    public string GetChapter(int bookIDB, int chapterNo);

}

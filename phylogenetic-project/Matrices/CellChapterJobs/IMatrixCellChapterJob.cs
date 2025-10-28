using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public interface IMatrixCellChapterJob<T_FieldData>
{
    List<int> bookIDBs { get; set; }
    List<int> chapters { get; set; }
    T_FieldData Calculate(int idx_idb1, int idx_idb2, int idx_chapter);
    decimal MergeChapters(T_FieldData[] chaptersList);
}

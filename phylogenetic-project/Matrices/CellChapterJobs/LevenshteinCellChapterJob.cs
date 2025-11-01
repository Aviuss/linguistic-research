using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public class LevenshteinCellChapterJob: IMatrixCellChapterJob<LevenshteinIndividualDataInt>
{
    public List<int> bookIDBs { get; set; }  = new List<int>();
    public List<int> chapters { get; set; } = new List<int>();

    Persistance.IGetChapter GetChapterConstruct;

    public LevenshteinCellChapterJob(Persistance.IGetChapter getChapterConstruct)
    {
        GetChapterConstruct = getChapterConstruct;
    }

    public LevenshteinIndividualDataInt Calculate(int idx_idb1, int idx_idb2, int idx_chapter)
    {
        int idb1 = bookIDBs[idx_idb1];
        int idb2 = bookIDBs[idx_idb2];
        int chapterNo = chapters[idx_chapter];
        string text_idb1 = GetChapterConstruct.GetChapter(idb1, chapterNo);
        string text_idb2 = GetChapterConstruct.GetChapter(idb2, chapterNo);

        return new LevenshteinIndividualDataInt(
            Algorithms.Levenshtein.Distance(text_idb1, text_idb2),
            Math.Max(text_idb1.Length, text_idb2.Length)
        );
    }

    public decimal MergeChapters(LevenshteinIndividualDataInt[] chaptersList)
    {
        if (chaptersList.Length != chapters.Count)
        {
            throw new ArgumentException("Lists must be the same length.");
        }

        int sumDistance = 0;
        int sumLength = 0;

        for (int i = 0; i < chaptersList.Length; i++)
        {
            sumDistance += chaptersList[i].levensthein_distance;
            sumLength += chaptersList[i].max_chapter_length;
        }

        return sumDistance / (decimal)sumLength;
    }

}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public class IPAFirstSingularChoiceLevenshteinCellChapterJob: IMatrixCellChapterJob<LevenshteinIndividualDataInt>
{
    public List<int> bookIDBs { get; set; }  = new List<int>();
    public List<int> chapters { get; set; } = new List<int>();

    Persistance.IGetChapter GetChapterConstruct;

    public IPAFirstSingularChoiceLevenshteinCellChapterJob(Persistance.IGetChapter getChapterConstruct)
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

        Persistance.LanguageRules? ipaRule_idb1 = Program.listOfLanguageRules?.Find(element => element.IdbCompatible.Contains(idb1));
        ArgumentNullException.ThrowIfNull(ipaRule_idb1);
        Persistance.LanguageRules? ipaRule_idb2 = Program.listOfLanguageRules?.Find(element => element.IdbCompatible.Contains(idb2));
        ArgumentNullException.ThrowIfNull(ipaRule_idb2);

        var ipaText_idb1 = StaticMethods.IPA.ConvertToIpa(
            text_idb1,
            ipaRule_idb1
        );
        
        var ipaText_idb2 = StaticMethods.IPA.ConvertToIpa(
            text_idb2,
            ipaRule_idb2
        );

        
        return Algorithms.LevenshteinIPAFirstSingularChoice.Calculate(ipaText_idb1, ipaText_idb2);
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

        return (decimal)sumDistance / sumLength;
    }

}


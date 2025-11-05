using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public class IPARandomChoiceLevenshteinCellChapterJobWithCustomIpaDistance: IMatrixCellChapterJob<LevenshteinIndividualDataDecimal>
{
    public List<int> bookIDBs { get; set; }  = new List<int>();
    public List<int> chapters { get; set; } = new List<int>();

    Persistance.IGetChapter GetChapterConstruct;
    long randomSize = 100;
    Persistance.IpaDistanceProvider ipaDistanceElement;
    bool parallelExecution;

    public IPARandomChoiceLevenshteinCellChapterJobWithCustomIpaDistance(Persistance.IGetChapter getChapterConstruct, long randomSize_, Persistance.IpaDistanceProvider ipaDistanceElement_, bool parallelExecution_ = false)
    {
        GetChapterConstruct = getChapterConstruct;
        randomSize = randomSize_;
        ipaDistanceElement = ipaDistanceElement_;
        parallelExecution = parallelExecution_;
    }

    public LevenshteinIndividualDataDecimal Calculate(int idx_idb1, int idx_idb2, int idx_chapter)
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

        if (parallelExecution)
        {
            return Algorithms.LevenshteinIPARandomChoiceAveragedWithCustomIpaDistanceInParallel.Calculate(ipaText_idb1, ipaText_idb2, ipaDistanceElement, randomSize);
        }        
        return Algorithms.LevenshteinIPARandomChoiceAveragedWithCustomIpaDistance.Calculate(ipaText_idb1, ipaText_idb2, ipaDistanceElement, randomSize);
    }

    public decimal MergeChapters(LevenshteinIndividualDataDecimal[] chaptersList)
    {
        if (chaptersList.Length != chapters.Count)
        {
            throw new ArgumentException("Lists must be the same length.");
        }

        decimal sumDistance = 0;
        decimal sumLength = 0;

        for (int i = 0; i < chaptersList.Length; i++)
        {
            sumDistance += chaptersList[i].levensthein_distance;
            sumLength += chaptersList[i].max_chapter_length;
        }

        return sumDistance / (decimal)sumLength;
    }

}


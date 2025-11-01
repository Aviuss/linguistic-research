using System;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public struct LevenshteinIndividualDataDecimal
{
    public decimal levensthein_distance;
    public decimal max_chapter_length;

    public LevenshteinIndividualDataDecimal(decimal distance, decimal MaxLength)
    {
        levensthein_distance = distance;
        max_chapter_length = MaxLength;
    }

    public LevenshteinIndividualDataDecimal()
    {
        levensthein_distance = (decimal)0;
        max_chapter_length = (decimal)1;
    }
}
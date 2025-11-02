using System;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public struct LevenshteinIndividualDataDecimal
{
    public decimal levensthein_distance { get; set; }
    public decimal max_chapter_length { get; set; }

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
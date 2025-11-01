using System;

namespace phylogenetic_project.Matrices.CellChapterJobs;

public struct LevenshteinIndividualDataInt
{
    public int levensthein_distance;
    public int max_chapter_length;

    public LevenshteinIndividualDataInt(int distance, int MaxLength)
    {
        levensthein_distance = distance;
        max_chapter_length = MaxLength;
    }

    public LevenshteinIndividualDataInt()
    {
        levensthein_distance = 0;
        max_chapter_length = 1;
    }
}
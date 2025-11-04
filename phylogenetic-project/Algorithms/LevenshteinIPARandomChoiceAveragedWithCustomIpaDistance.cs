using System;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPARandomChoiceAveragedWithCustomIpaDistance
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, Persistance.IpaDistanceProvider ipaLetterDistanceDict, long randomSize = 10000)
    {
        decimal avgLevenshteinDistance = 0;
        long avgMaxDistance = 0;

        var enumerator = IPARandomChoiceGenerator.ReturnRandomChoice(inputText1, inputText2, randomSize);

        int count = 0;
        foreach (var (txt1String, txt2String) in enumerator)
        {
            count++;
            avgLevenshteinDistance += Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict);
            avgMaxDistance += Math.Max(txt1String.Length, txt2String.Length);
        }

        return new LevenshteinIndividualDataDecimal(
            avgLevenshteinDistance / count,
            (decimal)avgMaxDistance / count
        );
    }

}


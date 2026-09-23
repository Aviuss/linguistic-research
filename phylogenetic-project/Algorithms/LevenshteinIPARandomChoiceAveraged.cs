using System;
using System.Text;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPARandomChoiceAveraged
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, long randomSize = 10000)
    {
        long avgLevenshteinDistance = 0;
        long avgMaxDistance = 0;

        var enumerator = IPARandomChoiceGenerator.ReturnRandomChoice(inputText1, inputText2, randomSize);

        int count = 0;
        foreach (var (txt1String, txt2String) in enumerator)
        {
            int distance = Algorithms.Levenshtein.Distance(txt1String, txt2String);
            int maxLen = Math.Max(txt1String.Length, txt2String.Length);
            StaticMethods.VerboseLog.Combination(count, txt1String, txt2String, distance, maxLen);

            count++;
            avgLevenshteinDistance += distance;
            avgMaxDistance += maxLen;
        }

        return new LevenshteinIndividualDataDecimal(
            (decimal)avgLevenshteinDistance / count,
            (decimal)avgMaxDistance / count
        );
    }

}


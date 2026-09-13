using System;
using System.Globalization;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPARandomChoiceAveragedWithCustomIpaDistance
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, Persistance.IpaCustomLetterDistance ipaLetterDistanceDict, long randomSize = 10000)
    {
        decimal avgLevenshteinDistance = 0;
        long avgMaxDistance = 0;

        var enumerator = IPARandomChoiceGenerator.ReturnRandomChoice(inputText1, inputText2, randomSize);

        int count = 0;
        foreach (var (txt1String, txt2String) in enumerator)
        {
            count++;
            avgLevenshteinDistance += Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict);
            
            var txt1StringTrueLen = new StringInfo(txt1String).LengthInTextElements;
            var txt2StringTrueLen = new StringInfo(txt2String).LengthInTextElements;
            avgMaxDistance += Math.Max(txt1StringTrueLen, txt2StringTrueLen);
        }

        return new LevenshteinIndividualDataDecimal(
            avgLevenshteinDistance / count,
            (decimal)avgMaxDistance / count
        );
    }

}


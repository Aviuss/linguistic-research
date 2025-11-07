using System;
using System;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPAFirstSingularWithCustomIpaDistance
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, Persistance.IpaDistanceProvider ipaLetterDistanceDict)
    {   
        string txt1String = string.Join("", inputText1.Select(element => element[0]).ToList());
        string txt2String = string.Join("", inputText2.Select(element => element[0]).ToList());
     
        return new LevenshteinIndividualDataDecimal(
            Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict),
            (decimal)Math.Max(txt1String.Length, txt2String.Length)
        );
    }

}


using System;
using System;
using System.Globalization;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPAFirstSingularWithCustomIpaDistance
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, Persistance.IpaCustomLetterDistance ipaLetterDistanceDict)
    {   
        string txt1String = string.Join("", inputText1.Select(element => element[0]).ToList());
        string txt2String = string.Join("", inputText2.Select(element => element[0]).ToList());
     
        var txt1StringTrueLen = new StringInfo(txt1String).LengthInTextElements;
        var txt2StringTrueLen = new StringInfo(txt2String).LengthInTextElements;
    
        var maxLen = Math.Max(txt1StringTrueLen, txt2StringTrueLen);

        return new LevenshteinIndividualDataDecimal(
            Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict),
            (decimal)maxLen
        );
    }

}


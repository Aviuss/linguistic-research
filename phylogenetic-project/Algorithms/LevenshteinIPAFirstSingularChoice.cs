using System;
using System.Text;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPAFirstSingularChoice
{
    public static LevenshteinIndividualDataInt Calculate
        (List<string[]> inputText1, List<string[]> inputText2)
    {
        string txt1String = string.Join("", inputText1.Select(element => element[0]).ToList());
        string txt2String = string.Join("", inputText2.Select(element => element[0]).ToList());
        
        int distance = Algorithms.Levenshtein.Distance(txt1String, txt2String);
        int maxLen = Math.Max(txt1String.Length, txt2String.Length);
        StaticMethods.VerboseLog.Combination(0, txt1String, txt2String, distance, maxLen);

        return new LevenshteinIndividualDataInt(distance, maxLen);
    }

}


using System;
using System;
using System.Globalization;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPARandomChoiceAveragedWithCustomIpaDistanceInParallel
{
    public static LevenshteinIndividualDataDecimal Calculate(
        List<string[]> inputText1,
        List<string[]> inputText2,
        Persistance.IpaCustomLetterDistance ipaLetterDistanceDict,
        long randomSize = 10000,
        int maxDegreeOfParallelism = 2
    )
    {
        decimal avgLevenshteinDistance = 0;
        long avgMaxDistance = 0;

        var enumerator = StaticMethods.ReturnGroupEnumerable<(int, string, string)>.Return(
            5,
            IPARandomChoiceGenerator.ReturnRandomChoice(inputText1, inputText2, randomSize)
                .Select((texts, index) => (index, texts.Item1, texts.Item2))
        );

        object locker = new object();
        int count = 0;
        Parallel.ForEach(
            enumerator,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            listOfStringsPair =>
            {
                foreach (var (index, txt1String, txt2String) in listOfStringsPair)
                {
                    var distResult = Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict);
                    var txt1StringTrueLen = new StringInfo(txt1String).LengthInTextElements;
                    var txt2StringTrueLen = new StringInfo(txt2String).LengthInTextElements;
                
                    var maxLen = Math.Max(txt1StringTrueLen, txt2StringTrueLen);
                    StaticMethods.VerboseLog.Combination(index, txt1String, txt2String, distResult, maxLen);

                    lock (locker)
                    {
                        //Console.WriteLine("Calculated now " + count + " on " + StaticMethods.CpuCoreHelper.GetCurrentCore().ToString());
                        count += 1;
                        avgLevenshteinDistance += distResult;
                        avgMaxDistance += maxLen;
                    }
                }
            }
        );
        
        return new LevenshteinIndividualDataDecimal(
            avgLevenshteinDistance / count,
            (decimal)avgMaxDistance / count
        );
    }

}


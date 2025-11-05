using System;
using System;
using phylogenetic_project.Matrices.CellChapterJobs;

namespace phylogenetic_project.Algorithms;

public class LevenshteinIPARandomChoiceAveragedWithCustomIpaDistanceInParallel
{
    public static LevenshteinIndividualDataDecimal Calculate
        (List<string[]> inputText1, List<string[]> inputText2, Persistance.IpaDistanceProvider ipaLetterDistanceDict, long randomSize = 10000)
    {
        decimal avgLevenshteinDistance = 0;
        long avgMaxDistance = 0;

        var enumerator = StaticMethods.ReturnGroupEnumerable<(string, string)>.Return(
            5,
            IPARandomChoiceGenerator.ReturnRandomChoice(inputText1, inputText2, randomSize)
        );

        object locker = new object();
        int count = 0;
        Parallel.ForEach(
            enumerator,
            new ParallelOptions { MaxDegreeOfParallelism = (int)Math.Ceiling(Environment.ProcessorCount * 1.5) },
            listOfStringsPair =>
            {
                foreach (var (txt1String, txt2String) in listOfStringsPair)
                {
                    var distResult = Algorithms.LevenshteinCustomIpaDistance.Distance(txt1String, txt2String, ipaLetterDistanceDict);
                    var maxLen = Math.Max(txt1String.Length, txt2String.Length);

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


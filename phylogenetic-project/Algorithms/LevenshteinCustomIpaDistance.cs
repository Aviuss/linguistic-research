using System;
using System.Globalization;

namespace phylogenetic_project.Algorithms;

public class LevenshteinCustomIpaDistance
{
    public static decimal Distance(string a, string b, Persistance.IpaDistanceProvider ipaLetterDistanceDict)
    {
        var aTrueLen = new StringInfo(a).LengthInTextElements;
        var bTrueLen = new StringInfo(b).LengthInTextElements;

        if (Math.Max(aTrueLen, bTrueLen) == 0) { return 0; }

        decimal[,] d = new decimal[aTrueLen + 1, bTrueLen + 1];

        for (int i = 0; i < aTrueLen + 1; i++)
        {
            d[i, 0] = i;
        }

        for (int i = 0; i < bTrueLen + 1; i++)
        {
            d[0, i] = i;
        }

        foreach (var (aEl, ai) in EnumerateVisualIPAText(a))
        {
            foreach (var (bEl, bj) in EnumerateVisualIPAText(b))
            {
                var i = ai + 1;
                var j = bj + 1;

                d[i, j] = Math.Min(
                    (decimal)Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + ipaLetterDistanceDict[aEl, bEl]
                );
            }
        }

        return d[aTrueLen, bTrueLen];
    }

    public static decimal DistanceNormalised(string a, string b, Persistance.IpaDistanceProvider ipaLetterDistanceDict)
    {
        var aTrueLen = new StringInfo(a).LengthInTextElements;
        var bTrueLen = new StringInfo(b).LengthInTextElements;
        var max = Math.Max(aTrueLen, bTrueLen);
        if (max == 0) { return 0; }
        return (decimal)Distance(a, b, ipaLetterDistanceDict) / max;
    }

    public static IEnumerable<(string, int)> EnumerateVisualIPAText(string input)
    {
        int index = -1;
        var enumerator = StringInfo.GetTextElementEnumerator(input);
        while (enumerator.MoveNext())
        {
            index++;
            yield return (enumerator.GetTextElement(), index);
        }
    }
}

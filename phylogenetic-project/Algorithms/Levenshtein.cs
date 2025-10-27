using System;

namespace phylogenetic_project.Algorithms;

public static class Levenshtein
{
    public static int Distance(string a, string b)
    {
        return Quickenshtein.Levenshtein.GetDistance(
            a,
            b,
            Quickenshtein.CalculationOptions.DefaultWithThreading
        );
    }

    public static decimal DistanceNormalised(string a, string b)
    {
        int dist = Quickenshtein.Levenshtein.GetDistance(
            a,
            b,
            Quickenshtein.CalculationOptions.DefaultWithThreading
        );

        return (decimal)dist / Math.Max(a.Length, b.Length);
    }
}

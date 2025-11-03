using System;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Testing.Algorithms;

public class LevenshteinCustomIpaDistance
{   
    phylogenetic_project.Persistance.IpaDistanceProvider ipaDict = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..", @"data and results", "json settings", "ipa_letter_distance.csv"));

    [Theory]
    [InlineData("ʊ̃", "k", 1)]
    [InlineData("ʊ̃", "ɑ", 0.55)]
    [InlineData("ʊ̃", "ɑk", 1.55)]
    [InlineData("ʊ̃", "kk", 2)]
    [InlineData("aaa", "aka", 1)]
    [InlineData("aaa", "äkä", 1.1)]
    public void DistanceCheck(string a, string b, decimal result)
    {
        var dist = phylogenetic_project.Algorithms.LevenshteinCustomIpaDistance.Distance(a, b, ipaDict);

        Assert.Equal(result, dist, precision: 8);
    }

    
    [Theory]
    [InlineData("ʊ̃", "k", 1, 1)]
    [InlineData("ʊ̃", "ɑ", 0.55, 1)]
    [InlineData("ʊ̃", "ɑk", 1.55, 2)]
    [InlineData("ʊ̃", "kk", 2, 2)]
    [InlineData("aaa", "aka", 1, 3)]
    [InlineData("aaa", "äkä", 1.1, 3)]
    public void DistanceNormalizedCheck(string a, string b, decimal res_dist, int max)
    {
        var dist = phylogenetic_project.Algorithms.LevenshteinCustomIpaDistance.DistanceNormalised(a, b, ipaDict);

        Assert.Equal((decimal)res_dist / max, dist, precision: 8);
    }

}


using Xunit;
namespace Testing.Algorithms;

public class Levenshtein
{
    [Theory]
    [InlineData("kotlet", "ketlot", 2)]
    [InlineData("kotl�t", "kotlet", 1)]
    [InlineData("kotl�t", "kotl�", 1)]
    [InlineData("kotl�t", "kotlt�", 2)]
    public void DistanceCheck(string a, string b, int result)
    {
        var dist = phylogenetic_project.Algorithms.Levenshtein.Distance(a, b);

        Assert.Equal(result, dist);
    }

    [Theory]
    [InlineData("kotlet", "ketlot", 2, 6)]
    [InlineData("kotl�t", "kotlet", 1, 6)]
    [InlineData("kotl�t", "kotl�", 1, 6)]
    [InlineData("kotl�t", "kotlt�", 2, 6)]
    public void DistanceNormalizedCheck(string a, string b, int res_dist, int max)
    {
        var dist = phylogenetic_project.Algorithms.Levenshtein.DistanceNormalised(a, b);

        Assert.Equal((decimal)res_dist / max, dist);
    }
}

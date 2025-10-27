using Xunit;
namespace Testing.Algorithms;

public class Levenshtein
{
    [Theory]
    [InlineData("kotlet", "ketlot", 2)]
    [InlineData("kotlêt", "kotlet", 1)]
    [InlineData("kotlêt", "kotlê", 1)]
    [InlineData("kotlêt", "kotltê", 2)]
    public void DistanceCheck(string a, string b, int result)
    {
        var dist = phylogenetic_project.Algorithms.Levenshtein.Distance(a, b);

        Assert.Equal(result, dist);
    }

    [Theory]
    [InlineData("kotlet", "ketlot", 2, 6)]
    [InlineData("kotlêt", "kotlet", 1, 6)]
    [InlineData("kotlêt", "kotlê", 1, 6)]
    [InlineData("kotlêt", "kotltê", 2, 6)]
    public void DistanceNormalizedCheck(string a, string b, int res_dist, int max)
    {
        var dist = phylogenetic_project.Algorithms.Levenshtein.DistanceNormalised(a, b);

        Assert.Equal((decimal)res_dist / max, dist);
    }
}

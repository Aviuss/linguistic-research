using System;

namespace phylogenetic_project.StaticMethods;

public static class ReturnGroupEnumerable<T>
{
    public static IEnumerable<List<T>> Return(int maxGroupSize, IEnumerable<T> enumerableElement)
    {
        if (maxGroupSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxGroupSize), "Group size must be greater than zero.");


        List<T> returnList = new List<T>();
        foreach (var item in enumerableElement)
        {
            returnList.Add(item);

            if (returnList.Count == maxGroupSize)
            {
                yield return returnList;
                returnList = new List<T>();
            }
        }

        if (returnList.Count > 0)
            yield return returnList;
    }

}

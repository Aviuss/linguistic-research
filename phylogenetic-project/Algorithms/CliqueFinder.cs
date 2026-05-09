using System;
using System.Collections.Generic;
using System.Linq;

namespace phylogenetic_project.Algorithms;


class CliqueFinder<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _graph;

    public CliqueFinder()
    {
        _graph = new Dictionary<T, HashSet<T>>();
    }

    public void Add((T, T) edge)
    {
        var (u, v) = edge;
        if (!_graph.ContainsKey(u)) _graph[u] = new HashSet<T>();
        if (!_graph.ContainsKey(v)) _graph[v] = new HashSet<T>();
        _graph[u].Add(v);
        _graph[v].Add(u);
    }


    private List<List<T>> FindAllMaximalCliques()
    {
        var cliques = new List<List<T>>();
        var R = new HashSet<T>();
        var P = new HashSet<T>(_graph.Keys);
        var X = new HashSet<T>();

        BronKerbosch(R, P, X, cliques);
        return cliques;
    }

    private void BronKerbosch(HashSet<T> R, HashSet<T> P, HashSet<T> X,
                               List<List<T>> cliques)
    {
        if (P.Count == 0 && X.Count == 0)
        {
            cliques.Add(R.ToList());
            return;
        }

        T pivot = P.Union(X)
                   .OrderByDescending(u => P.Intersect(Neighbors(u)).Count())
                   .First();

        foreach (T v in P.Except(Neighbors(pivot)).ToList())
        {
            var neighbors = Neighbors(v);

            var newR = new HashSet<T>(R);
            newR.Add(v);

            BronKerbosch(
                newR,
                new HashSet<T>(P.Intersect(neighbors)),
                new HashSet<T>(X.Intersect(neighbors)),
                cliques
            );

            P.Remove(v);
            X.Add(v);
        }
    }
    
    public List<List<T>> FindAllCliques()
    {
        var maximal = FindAllMaximalCliques();
        var result  = new List<List<T>>();

        foreach (var clique in maximal)
        {
            foreach (var subset in GetSubsets(clique))
            {
                if (subset.Count <= 1)
                    continue;

                subset.Sort();
                bool cliqueIsInResult = false;
                
                foreach (var res in result)
                {
                    if (res.Count != subset.Count)
                        continue;

                    for (int i = 0; i < subset.Count; i++)
                    {
                        if (!EqualityComparer<T>.Default.Equals(subset[i], res[i]))
                        {
                            cliqueIsInResult = true;
                            break;
                        }
                    }

                    if (cliqueIsInResult)
                    {
                        break;
                    }

                }
                
                if (!cliqueIsInResult)
                    result.Add(subset);
            
            }
        }       
        return result.ToList();
    }

    private HashSet<T> Neighbors(T v) {
        HashSet<T>? n;
        _graph.TryGetValue(v, out n);
        ArgumentNullException.ThrowIfNull(n);
        return n;
    }

    private static IEnumerable<List<T>> GetSubsets(List<T> set)
    {
        if (set.Count == 0)
        {
            yield return new List<T>();
            yield break;
        }

        T first = set[0];
        List<T> rest = set.Skip(1).ToList();

        foreach (var subset in GetSubsets(rest))
        {
            yield return subset;

            var withFirst = new List<T>(subset) { first };
            yield return withFirst;
        }
    }

}
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

    // Add undirected edge between two T nodes
    public void Add((T, T) edge)
    {
        var (u, v) = edge;
        if (!_graph.ContainsKey(u)) _graph[u] = new HashSet<T>();
        if (!_graph.ContainsKey(v)) _graph[v] = new HashSet<T>();
        _graph[u].Add(v);
        _graph[v].Add(u);
    }

    // ─── Bron-Kerbosch with Pivot ────────────────────────────────────────────

    public List<List<T>> FindAllMaximalCliques()
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

            BronKerbosch(
                new HashSet<T>(R) { v },
                new HashSet<T>(P.Intersect(neighbors)),
                new HashSet<T>(X.Intersect(neighbors)),
                cliques
            );

            P.Remove(v);
            X.Add(v);
        }
    }

    // ─── All cliques (including non-maximal subsets) ─────────────────────────

    public List<List<T>> FindAllCliques()
    {
        var maximal = FindAllMaximalCliques();
        var seen    = new HashSet<string>();
        var result  = new List<List<T>>();

        foreach (var clique in maximal)
            foreach (var subset in GetSubsets(clique))
                if (subset.Count >= 2)
                {
                    var key = string.Join(",", subset.Select(n => n.ToString()).OrderBy(x => x));
                    if (seen.Add(key))
                        result.Add(subset);
                }

        return result.OrderBy(c => c.Count).ToList();
    }

    // ─── Maximum clique ───────────────────────────────────────────────────────

    public List<T> FindMaximumClique() =>
        FindAllMaximalCliques()
            .OrderByDescending(c => c.Count)
            .FirstOrDefault() ?? new List<T>();

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HashSet<T> Neighbors(T v) =>
        _graph.TryGetValue(v, out var n) ? n : new HashSet<T>();

    private static IEnumerable<List<T>> GetSubsets(List<T> set)
    {
        int n = set.Count;
        for (int mask = 1; mask < (1 << n); mask++)
        {
            var subset = new List<T>();
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    subset.Add(set[i]);
            yield return subset;
        }
    }
}
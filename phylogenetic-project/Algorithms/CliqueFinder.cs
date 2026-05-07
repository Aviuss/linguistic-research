using System;
using System.Collections.Generic;
using System.Linq;

namespace phylogenetic_project.Algorithms;

class CliqueFinder
{
    private readonly Dictionary<int, HashSet<int>> _graph;

    public CliqueFinder()
    {
        _graph = new Dictionary<int, HashSet<int>>();
    }

    // Add undirected edge
    public void AddEdge(int u, int v)
    {
        if (!_graph.ContainsKey(u)) _graph[u] = new HashSet<int>();
        if (!_graph.ContainsKey(v)) _graph[v] = new HashSet<int>();
        _graph[u].Add(v);
        _graph[v].Add(u);
    }

    // ─── Bron-Kerbosch with Pivot ────────────────────────────────────────────

    public List<List<int>> FindAllMaximalCliques()
    {
        var cliques = new List<List<int>>();
        var R = new HashSet<int>();
        var P = new HashSet<int>(_graph.Keys);
        var X = new HashSet<int>();

        BronKerbosch(R, P, X, cliques);
        return cliques;
    }

    private void BronKerbosch(HashSet<int> R, HashSet<int> P, HashSet<int> X,
                               List<List<int>> cliques)
    {
        if (P.Count == 0 && X.Count == 0)
        {
            cliques.Add(R.OrderBy(v => v).ToList()); // found a maximal clique
            return;
        }

        // Choose pivot u that maximizes |P ∩ N(u)|
        int pivot = P.Union(X)
                     .OrderByDescending(u => P.Intersect(Neighbors(u)).Count())
                     .First();

        // Iterate over P \ N(pivot)
        foreach (int v in P.Except(Neighbors(pivot)).ToList())
        {
            var neighbors = Neighbors(v);

            BronKerbosch(
                new HashSet<int>(R) { v },
                new HashSet<int>(P.Intersect(neighbors)),
                new HashSet<int>(X.Intersect(neighbors)),
                cliques
            );

            P.Remove(v);
            X.Add(v);
        }
    }

    // ─── All cliques (including non-maximal subsets) ─────────────────────────

    public List<List<int>> FindAllCliques()
    {
        var maximal = FindAllMaximalCliques();
        var all = new HashSet<string>(); // use string key for dedup
        var result = new List<List<int>>();

        foreach (var clique in maximal)
            foreach (var subset in GetSubsets(clique))
                if (subset.Count >= 2)
                {
                    var key = string.Join(",", subset.OrderBy(x => x));
                    if (all.Add(key))
                        result.Add(subset);
                }

        return result.OrderBy(c => c.Count).ThenBy(c => c[0]).ToList();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HashSet<int> Neighbors(int v) =>
        _graph.TryGetValue(v, out var n) ? n : new HashSet<int>();

    private static IEnumerable<List<int>> GetSubsets(List<int> set)
    {
        int n = set.Count;
        for (int mask = 1; mask < (1 << n); mask++)
        {
            var subset = new List<int>();
            for (int i = 0; i < n; i++)
                if ((mask & (1 << i)) != 0)
                    subset.Add(set[i]);
            yield return subset;
        }
    }
}
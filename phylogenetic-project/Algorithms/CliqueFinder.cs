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

    public int GetGraphCount() => _graph.Count;

    public void Add((T, T) edge)
    {
        var (u, v) = edge;
        if (!_graph.ContainsKey(u)) _graph[u] = new HashSet<T>();
        if (!_graph.ContainsKey(v)) _graph[v] = new HashSet<T>();
        _graph[u].Add(v);
        _graph[v].Add(u);
    }


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
    
    private HashSet<T> Neighbors(T v) {
        HashSet<T>? n;
        _graph.TryGetValue(v, out n);
        ArgumentNullException.ThrowIfNull(n);
        return n;
    }

}
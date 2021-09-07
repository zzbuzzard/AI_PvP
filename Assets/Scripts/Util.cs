using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<U, V> : IComparable<Pair<U, V>> where U : IComparable<U> where V : IComparable<V>
{
    public U fst;
    public V snd;

    public Pair(U u, V v)
    {
        fst = u;
        snd = v;
    }

    public int CompareTo(Pair<U, V> other)
    {
        int p = fst.CompareTo(other.fst);
        if (p == 0) return snd.CompareTo(other.snd);
        return p;
    }
}

public static class Util
{
    // Produces a new instance of the list
    public static void Shuffle<T>(List<T> a)
    {
        List<T> b = new List<T>();
        while (a.Count > 0)
        {
            int ind = UnityEngine.Random.Range(0, a.Count);
            T t = a[ind];
            a.RemoveAt(ind);
            b.Add(t);
        }
        foreach (T t in b) a.Add(t);
    }
}
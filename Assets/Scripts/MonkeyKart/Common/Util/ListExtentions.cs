using System;
using System.Collections.Generic;

public static class ListExtensions
{
    public static T GetWithLoopedIndex<T>(this List<T> list, int index)
    {
        int adjustedIndex = ((index % list.Count) + list.Count) % list.Count;
        return list[adjustedIndex];
    }
}
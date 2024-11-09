using System;
using System.Collections.Generic;

public static class ListExtensions
{
    static Random random = new Random();
    
    public static T GetWithLoopedIndex<T>(this List<T> list, int index)
    {
        int adjustedIndex = ((index % list.Count) + list.Count) % list.Count;
        return list[adjustedIndex];
    }
    
    public static T RandomGet<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            throw new InvalidOperationException("リストが空です");
        }

        int index = random.Next(list.Count);
        return list[index];
    }
}
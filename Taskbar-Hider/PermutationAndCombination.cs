using System;
using System.Collections.Generic;

namespace Taskbar_Hider;

public class PermutationAndCombination<T>
{
    /// <summary>
    /// 交换两个变量
    /// </summary>
    /// <param name="a">变量1</param>
    /// <param name="b">变量2</param>
    private static void Swap(ref T a, ref T b)
    {
        (a, b) = (b, a);
    }

    /// <summary>
    /// 递归算法求数组的组合(私有成员)
    /// </summary>
    /// <param name="list">返回的范型</param>
    /// <param name="t">所求数组</param>
    /// <param name="n">辅助变量</param>
    /// <param name="m">辅助变量</param>
    /// <param name="b">辅助数组</param>
    /// <param name="M">辅助变量M</param>
    private static void GetCombination(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
    {
        for (int i = n; i >= m; i--)
        {
            b[m - 1] = i - 1;
            if (m > 1)
            {
                GetCombination(ref list, t, i - 1, m - 1, b, M);
            }
            else
            {
                list ??= [];

                var temp = new T[M];
                for (int j = 0; j < b.Length; j++)
                {
                    temp[j] = t[b[j]];
                }

                list.Add(temp);
            }
        }
    }

    /// <summary>
    /// 递归算法求排列(私有成员)
    /// </summary>
    /// <param name="list">返回的列表</param>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    private static void GetPermutation(ref List<T[]> list, T[] t, int startIndex, int endIndex)
    {
        if (startIndex == endIndex)
        {
            list ??= [];

            var temp = new T[t.Length];
            t.CopyTo(temp, 0);
            list.Add(temp);
        }
        else
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                Swap(ref t[startIndex], ref t[i]);
                GetPermutation(ref list, t, startIndex + 1, endIndex);
                Swap(ref t[startIndex], ref t[i]);
            }
        }
    }

    /// <summary>
    /// 求从起始标号到结束标号的排列，其余元素不变
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    /// <returns>从起始标号到结束标号排列的范型</returns>
    public static List<T[]> GetPermutation(T[] t, int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex > t.Length - 1)
        {
            throw new ArgumentOutOfRangeException();
        }

        List<T[]> list = [];
        GetPermutation(ref list, t, startIndex, endIndex);
        return list;
    }

    /// <summary>
    /// 返回数组所有元素的全排列
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <returns>全排列的范型</returns>
    public static List<T[]> GetPermutation(T[] t)
    {
        return GetPermutation(t, 0, t.Length - 1);
    }

    /// <summary>
    /// 求数组中n个元素的排列
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的排列</returns>
    public static List<T[]> GetPermutation(T[] t, int n)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, t.Length);

        List<T[]> list = [];
        List<T[]> c = GetCombination(t, n);
        foreach (var t1 in c)
        {
            List<T[]> l = [];
            GetPermutation(ref l, t1, 0, n - 1);
            list.AddRange(l);
        }

        return list;
    }

    /// <summary>
    /// 求数组中n个元素的组合
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的组合的范型</returns>
    public static List<T[]> GetCombination(T[] t, int n)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, t.Length);

        int[] temp = new int[n];
        List<T[]> list = [];
        GetCombination(ref list, t, t.Length, n, temp, n);
        return list;
    }
}
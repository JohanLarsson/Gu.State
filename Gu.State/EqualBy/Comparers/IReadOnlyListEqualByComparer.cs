namespace Gu.State
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class IReadOnlyListEqualByComparer
    {
        internal static bool TryGet(Type type, MemberSettings settings, out EqualByComparer comparer)
        {
            if (type.Implements(typeof(IReadOnlyList<>)))
            {
                if (type.IsArray)
                {
                    comparer = (EqualByComparer)typeof(ArrayComparer<>).MakeGenericType(type.GetItemType())
                                                                  .GetField(nameof(ArrayComparer<int>.Default), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                                                  .GetValue(null);
                }
                else if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    comparer = (EqualByComparer)typeof(ListComparer<>).MakeGenericType(type.GetItemType())
                                                                      .GetField(nameof(ListComparer<int>.Default), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                                                      .GetValue(null);
                }
                else
                {
                    comparer = (EqualByComparer)typeof(Comparer<>).MakeGenericType(type.GetItemType())
                                                                  .GetField(nameof(Comparer<int>.Default), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                                                  .GetValue(null);
                }

                return true;
            }

            comparer = null;
            return false;
        }

        private class Comparer<T> : CollectionEqualByComparer<IReadOnlyList<T>,T>
        {
            public static Comparer<T> Default = new Comparer<T>();

            private Comparer()
            {
            }

            internal override bool Equals(IReadOnlyList<T> x, IReadOnlyList<T> y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                var comparer = settings.GetEqualByComparer(typeof(T));
                for (var i = 0; i < x.Count; i++)
                {
                    if (!comparer.Equals(x[i], y[i], settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class ListComparer<T> : CollectionEqualByComparer<List<T>, T>
        {
            public static ListComparer<T> Default = new ListComparer<T>();

            private ListComparer()
            {
            }

            internal override bool Equals(List<T> x, List<T> y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                var comparer = settings.GetEqualByComparer(typeof(T));
                for (var i = 0; i < x.Count; i++)
                {
                    if (!comparer.Equals(x[i], y[i], settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class ArrayComparer<T> : CollectionEqualByComparer<T[], T>
        {
            public static ArrayComparer<T> Default = new ArrayComparer<T>();

            private ArrayComparer()
            {
            }

            internal override bool Equals(T[] x, T[] y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }

                var comparer = settings.GetEqualByComparer(typeof(T));
                for (var i = 0; i < x.Length; i++)
                {
                    if (!comparer.Equals(x[i], y[i], settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
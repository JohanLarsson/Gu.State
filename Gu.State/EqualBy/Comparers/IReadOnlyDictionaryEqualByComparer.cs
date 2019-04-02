namespace Gu.State
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class IReadOnlyDictionaryEqualByComparer
    {
        private static readonly ConcurrentDictionary<Type, EqualByComparer> Cache = new ConcurrentDictionary<Type, EqualByComparer>();

        internal static bool TryGet(Type type, MemberSettings settings, out EqualByComparer comparer)
        {
            if (type.Implements(typeof(IReadOnlyDictionary<,>)))
            {
                var iDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)
                    ? type
                    : type.GetInterface("IReadOnlyDictionary`2");
                comparer = (EqualByComparer)typeof(Comparer<,>).MakeGenericType(iDict.GenericTypeArguments[0], iDict.GenericTypeArguments[1])
                                                               .GetField(nameof(Comparer<int, int>.Default), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                                                               .GetValue(null);
                return true;
            }

            comparer = null;
            return false;
        }

        private class Comparer<TKey, TValue> : EqualByComparer
        {
            public static readonly Comparer<TKey, TValue> Default = new Comparer<TKey, TValue>();

            private Comparer()
            {
            }

            /// <inheritdoc />
            public override bool Equals(object x, object y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                if (TryGetEitherNullEquals(x, y, out var result))
                {
                    return result;
                }

                return Equals((IReadOnlyDictionary<TKey, TValue>)x, (IReadOnlyDictionary<TKey, TValue>)y, settings, referencePairs);
            }

            private static bool Equals(IReadOnlyDictionary<TKey, TValue> x, IReadOnlyDictionary<TKey, TValue> y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                if (x.Count != y.Count)
                {
                    return false;
                }

                return ISetEqualByComparer.SetEquals(x.Keys, y.Keys,settings, referencePairs) &&
                       ValuesEquals(x, y, settings, referencePairs);
            }

            private static bool ValuesEquals(IReadOnlyDictionary<TKey, TValue> x, IReadOnlyDictionary<TKey, TValue> y, MemberSettings settings, ReferencePairCollection referencePairs)
            {
                var comparer = settings.GetEqualByComparer(typeof(TValue), checkReferenceHandling: true);
                foreach (var key in x.Keys)
                {
                    if (!y.TryGetValue(key, out var yv))
                    {
                        return false;
                    }

                    if (!comparer.Equals(x[key], yv, settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
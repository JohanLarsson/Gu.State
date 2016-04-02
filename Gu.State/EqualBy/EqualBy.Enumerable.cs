﻿namespace Gu.State
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public static partial class EqualBy
    {
        private static bool EnumerableEquals<TSetting>(
            object x,
            object y,
            Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
            TSetting settings,
            ReferencePairCollection referencePairs)
            where TSetting : class, IMemberSettings
        {
            Debug.Assert(settings.ReferenceHandling != ReferenceHandling.Throw, "Should not get here");

            IList xl;
            IList yl;
            if (Collection.TryCastAs(x, y, out xl, out yl))
            {
                return Collection.Equals(xl, yl, compareItem, settings, referencePairs);
            }

            IDictionary xd;
            IDictionary yd;
            if (Collection.TryCastAs(x, y, out xd, out yd))
            {
                return Collection.Equals(xd, yd, compareItem, settings, referencePairs);
            }

            if (Collection.IsSets(x, y))
            {
                return Collection.SetEquals(x, y, compareItem, settings, referencePairs);
            }

            IEnumerable xe;
            IEnumerable ye;
            if (Collection.TryCastAs(x, y, out xe, out ye))
            {
                return Collection.Equals(xe, ye, compareItem, settings, referencePairs);
            }

            var message = "There is a bug in the library as it:\r\n" +
                          $"Could not compare enumerables of type {x.GetType().PrettyName()}";
            throw new InvalidOperationException(message);
        }

        private static class Collection
        {
            internal static bool IsSets(object x, object y)
            {
                if (x?.GetType().Implements(typeof(ISet<>)) != true || y?.GetType().Implements(typeof(ISet<>)) != true)
                {
                    return false;
                }

                return x.GetType().GetItemType() == y.GetType().GetItemType();
            }

            internal static bool TryCastAs<T>(object x, object y, out T xResult, out T yResult)
            {
                if (x is T && y is T)
                {
                    xResult = (T)x;
                    yResult = (T)y;
                    return true;
                }

                xResult = default(T);
                yResult = default(T);
                return false;
            }

            internal static bool Equals<TSetting>(
                IList x,
                IList y,
                Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
                TSetting settings,
                ReferencePairCollection referencePairs)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Count != y.Count)
                {
                    return false;
                }

                for (int i = 0; i < x.Count; i++)
                {
                    var xv = x[i];
                    var yv = y[i];
                    if (referencePairs?.Contains(xv, yv) == true)
                    {
                        continue;
                    }

                    if (!compareItem(xv, yv, settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal static bool Equals<TSetting>(
                IDictionary x,
                IDictionary y,
                Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
                TSetting settings,
                ReferencePairCollection referencePairs)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                if (x.Count != y.Count)
                {
                    return false;
                }

                foreach (var key in x.Keys)
                {
                    if (!y.Contains(key))
                    {
                        return false;
                    }

                    var xv = x[key];
                    var yv = y[key];
                    if (referencePairs?.Contains(xv, yv) == true)
                    {
                        continue;
                    }

                    if (!compareItem(xv, yv, settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }

            internal static bool SetEquals<TSetting>(
                object x,
                object y,
                Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
                TSetting settings,
                ReferencePairCollection referencePairs)
                where TSetting : IMemberSettings
            {
                var setEqualsMethod = x.GetType().GetMethod("SetEquals", BindingFlags.Public | BindingFlags.Instance);
                Debug.Assert(setEqualsMethod != null, "setEqualsMethod == null");
                var setEquals = (bool)setEqualsMethod.Invoke(x, new[] { y });
                if (!setEquals)
                {
                    return false;
                }

                var xe = ((IEnumerable)x).Cast<object>().OrderBy(i => i.GetHashCode());
                var ye = ((IEnumerable)y).Cast<object>().OrderBy(i => i.GetHashCode());
                return Equals(xe, ye, compareItem, settings, referencePairs);
            }

            internal static bool Equals<TSetting>(
                IEnumerable x,
                IEnumerable y,
                Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
                TSetting settings,
                ReferencePairCollection referencePairs)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                foreach (var pair in new PaddedPairs(x, y))
                {
                    if (referencePairs?.Contains(pair.X, pair.Y) == true)
                    {
                        continue;
                    }

                    if (!compareItem(pair.X, pair.Y, settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}

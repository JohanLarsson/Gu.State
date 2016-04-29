namespace Gu.State
{
    using System;

    /// <inheritdoc />
    public class ArrayEqualByComparer : EqualByComparer
    {
        /// <summary>The default instance.</summary>
        public static readonly ArrayEqualByComparer Default = new ArrayEqualByComparer();

        private ArrayEqualByComparer()
        {
        }

        public static bool TryGetOrCreate(object x, object y, out EqualByComparer comparer)
        {
            if (x is Array && y is Array)
            {
                comparer = Default;
                return true;
            }

            comparer = null;
            return false;
        }

        /// <inheritdoc />
        public override bool Equals<TSetting>(
            object x,
            object y,
            Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
            TSetting settings,
            ReferencePairCollection referencePairs)
        {
            bool result;
            if (TryGetEitherNullEquals(x, y, out result))
            {
                return result;
            }

            return Equals((Array)x, (Array)y, compareItem, settings, referencePairs);
        }

        private static bool Equals<TSetting>(
            Array x,
            Array y,
            Func<object, object, TSetting, ReferencePairCollection, bool> compareItem,
            TSetting settings,
            ReferencePairCollection referencePairs)
            where TSetting : IMemberSettings
        {
            if (x.Length != y.Length || x.Rank != y.Rank)
            {
                return false;
            }

            for (var i = 0; i < x.Rank; i++)
            {
                if (x.GetLowerBound(i) != y.GetLowerBound(i) ||
                    x.GetUpperBound(i) != y.GetUpperBound(i))
                {
                    return false;
                }
            }

            var isEquatable = settings.IsEquatable(x.GetType().GetItemType());
            if (settings.ReferenceHandling == ReferenceHandling.References)
            {
                return isEquatable
                           ? ItemsEquals(x, y, object.Equals)
                           : ItemsEquals(x, y, ReferenceEquals);
            }

            return isEquatable
                       ? ItemsEquals(x, y, object.Equals)
                       : ItemsEquals(x, y, (xi, yi) => compareItem(xi, yi, settings, referencePairs));
        }

        private static bool ItemsEquals(Array x, Array y, Func<object, object, bool> compare)
        {
            var xe = x.GetEnumerator();
            var ye = y.GetEnumerator();
            while (xe.MoveNext() && ye.MoveNext())
            {
                if (!compare(xe.Current, ye.Current))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
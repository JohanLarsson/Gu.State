﻿namespace Gu.State
{
    using System;
    using System.Collections.Generic;

    /// <summary>A comparer that compares by member or index.</summary>
    internal abstract class EqualByComparer
    {
        internal abstract bool CanHaveReferenceLoops { get; }

        internal static EqualByComparer Create(Type type, MemberSettings settings)
        {
            if (NullableEqualByComparer.TryGet(type, out var comparer) ||
                EquatableEqualByComparer.TryGet(type, settings, out comparer) ||
                ISetEqualByComparer.TryCreate(type, settings, out comparer) ||
                IReadOnlyListEqualByComparer.TryCreate(type, settings, out comparer) ||
                ArrayEqualByComparer.TryGet(type, settings, out comparer) ||
                IReadOnlyDictionaryEqualByComparer.TryCreate(type, settings, out comparer) ||
                IEnumerableEqualByComparer.TryGet(type, settings, out comparer) ||
                ComplexTypeEqualByComparer.TryGet(type, settings, out comparer))
            {
                return comparer;
            }

            return new ErrorEqualByComparer(type, new NotSupported(type, $"Could not find an EqualByComparer<{type.PrettyName()}>"));
        }

        internal abstract bool TryGetError(MemberSettings settings, out Error error);

        /// <summary>
        /// Compare <paramref name="x"/> with <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="settings">The settings that specifies how comparison is performed.</param>
        /// <param name="referencePairs">
        /// The already compared items. This is used to stop infinite recursion when there are reference loops.
        /// </param>
        /// <returns>True if <paramref name="x"/> and <paramref name="y"/> are equal.</returns>
        internal abstract bool Equals(object x, object y, MemberSettings settings, HashSet<ReferencePairStruct> referencePairs);

        /// <summary>Convenience method for checking equality if either or both <paramref name="x"/> and <paramref name="y"/> are null.</summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="result">The result.</param>
        /// <returns>True if any of <paramref name="x"/> and <paramref name="y"/> is null.</returns>
        protected static bool TryGetEitherNullEquals(object x, object y, out bool result)
        {
            if (ReferenceEquals(x, y))
            {
                result = true;
                return true;
            }

            if (x is null || y is null)
            {
                result = false;
                return true;
            }

            if (x.GetType() != y.GetType())
            {
                result = false;
                return true;
            }

            result = false;
            return false;
        }
    }
}

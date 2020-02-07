﻿namespace Gu.State
{
    using System.Collections.Generic;

    internal sealed class ReferenceEqualByComparer : EqualByComparer
    {
        public static readonly ReferenceEqualByComparer Default = new ReferenceEqualByComparer();

        internal override bool CanHaveReferenceLoops => false;

        internal override bool TryGetError(MemberSettings settings, out Error error)
        {
            error = null;
            return false;
        }

        internal override bool Equals(object x, object y, MemberSettings settings, HashSet<ReferencePairStruct> referencePairs)
        {
            return ReferenceEquals(x, y);
        }
    }
}
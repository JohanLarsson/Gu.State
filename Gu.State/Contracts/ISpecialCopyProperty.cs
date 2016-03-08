﻿namespace Gu.State
{
    using System.Reflection;

    public interface ISpecialCopyProperty
    {
        PropertyInfo Property { get; }

        void CopyValue(object source, object target);
    }
}
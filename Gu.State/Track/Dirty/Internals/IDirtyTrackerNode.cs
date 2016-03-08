﻿namespace Gu.State
{
    using System;
    using System.Reflection;

    internal interface IDirtyTrackerNode : IDisposable
    {
        bool IsDirty { get; }

        PropertyInfo PropertyInfo { get; }
    }
}
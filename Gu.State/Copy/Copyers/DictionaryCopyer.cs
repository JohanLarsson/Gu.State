﻿namespace Gu.State
{
    using System;
    using System.Collections;

    public class DictionaryCopyer : ICopyer
    {
        public static readonly DictionaryCopyer Default = new DictionaryCopyer();

        private DictionaryCopyer()
        {
        }

        public static bool TryGetOrCreate(object x, object y, out ICopyer comparer)
        {
            if (Is.Type<IDictionary>(x, y))
            {
                comparer = Default;
                return true;
            }

            comparer = null;
            return false;
        }

        public void Copy<TSettings>(
            object source,
            object target,
            Func<object, object, TSettings, ReferencePairCollection, object> copyItem,
            TSettings settings,
            ReferencePairCollection referencePairs)
            where TSettings : class, IMemberSettings
        {
            Copy((IDictionary)source, (IDictionary)target, copyItem, settings, referencePairs);
        }

        internal static void Copy<TSettings>(
            IDictionary source,
            IDictionary target,
            Func<object, object, TSettings, ReferencePairCollection, object> copyItem,
            TSettings settings,
            ReferencePairCollection referencePairs)
            where TSettings : class, IMemberSettings
        {
            using (var toRemove = ListPool<object>.Borrow())
            {
                foreach (var key in target.Keys)
                {
                    if (!source.Contains(key))
                    {
                        toRemove.Value.Add(key);
                    }
                    else
                    {
                        // Synchronize key?
                    }
                }

                foreach (var key in toRemove.Value)
                {
                    target.Remove(key);
                }
            }

            foreach (var key in source.Keys)
            {
                var sv = source[key];
                var tv = target.ElementAtOrDefault(key);
                var copy = State.Copy.Item(sv, tv, copyItem, settings, referencePairs, settings.IsImmutable(sv.GetType()));
                target[key] = copy;
            }
        }
    }
}
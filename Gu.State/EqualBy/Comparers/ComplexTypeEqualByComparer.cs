namespace Gu.State
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    internal static class ComplexTypeEqualByComparer
    {
        internal static bool TryGet(Type type, MemberSettings settings, out EqualByComparer comparer)
        {
            comparer = Activator.CreateInstance<EqualByComparer>(
                typeof(Comparer<>).MakeGenericType(type),
                new object[]
                {
                    ImmutableArray.Create(settings.GetEffectiveMembers(type)
                                                  .Concat(IllegalIndexers())
                                                  .Select(m => MemberEqualByComparer.Create(m, settings))),
                });
            return true;

            IEnumerable<MemberInfo> IllegalIndexers()
            {
                foreach (var candidate in type.GetProperties(settings.BindingFlags))
                {
                    if (candidate.IsIndexer() &&
                        !settings.IsIgnoringMember(candidate))
                    {
                        yield return candidate;
                    }
                }
            }
        }

        [DebuggerDisplay("ComplexTypeEqualByComparer<{typeof(T).PrettyName()}>")]
        private sealed class Comparer<T> : EqualByComparer<T>
        {
            private readonly ImmutableArray<MemberEqualByComparer> memberComparers;
            private TypeErrors lazyTypeErrors;
            private bool? lazyCanHaveReferenceLoops;

            internal Comparer(ImmutableArray<MemberEqualByComparer> memberComparers)
            {
                this.memberComparers = memberComparers;
            }

            internal override bool CanHaveReferenceLoops
            {
                get
                {
                    if (this.lazyCanHaveReferenceLoops is null)
                    {
                        // Setting it to true here to detect reference loop via recursion.
                        this.lazyCanHaveReferenceLoops = true;
                        this.lazyCanHaveReferenceLoops = this.memberComparers.Any(x => x.CanHaveReferenceLoops);
                    }

                    return this.lazyCanHaveReferenceLoops.Value;
                }
            }

            internal override bool TryGetError(MemberSettings settings, out Error error)
            {
                if (this.lazyTypeErrors is null)
                {
                    var errors = new List<Error>();
                    this.lazyTypeErrors = new TypeErrors(typeof(T), errors);
                    foreach (var memberComparer in this.memberComparers)
                    {
                        if (memberComparer.TryGetError(settings, out var memberErrors))
                        {
                            errors.Add(new MemberErrors(MemberPath.Create(memberComparer.Member), memberErrors));
                        }
                    }
                }

                if (this.lazyTypeErrors.Errors.Count == 0)
                {
                    error = null;
                    return false;
                }

                error = this.lazyTypeErrors;
                return true;
            }

            internal override bool Equals(object x, object y, MemberSettings settings, HashSet<ReferencePairStruct> referencePairs)
            {
                if (TryGetEitherNullEquals(x, y, out var result))
                {
                    return result;
                }

                return this.Equals((T)x, (T)y, settings, referencePairs);
            }

            internal override bool Equals(T x, T y, MemberSettings settings, HashSet<ReferencePairStruct> referencePairs)
            {
                if (referencePairs != null &&
                    ReferencePairStruct.TryCreate(x, y, out var pair))
                {
                    if (referencePairs.Add(pair) == false)
                    {
                        return true;
                    }

                    for (var i = 0; i < this.memberComparers.Count; i++)
                    {
                        if (!this.memberComparers[i].Equals(x, y, settings, referencePairs))
                        {
                            referencePairs.Remove(pair);
                            return false;
                        }
                    }

                    referencePairs.Remove(pair);
                    return true;
                }

                for (var i = 0; i < this.memberComparers.Count; i++)
                {
                    if (!this.memberComparers[i].Equals(x, y, settings, referencePairs))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}

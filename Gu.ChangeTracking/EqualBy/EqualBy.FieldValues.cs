namespace Gu.ChangeTracking
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text;

    public static partial class EqualBy
    {
        public static bool FieldValues<T>(T x, T y, BindingFlags bindingFlags)
        {
            var settings = EqualByFieldsSettings.GetOrCreate(bindingFlags);
            return FieldValues(x, y, settings);
        }

        /// <summary>
        /// Compares x and y for equality using field values.
        /// If a type implements IList the items of the list are compared
        /// </summary>
        /// <param name="referenceHandling">
        /// Specifies how reference types are compared.
        /// Structural compares field values recursively.
        /// </param>
        public static bool FieldValues<T>(T x, T y, ReferenceHandling referenceHandling)
        {
            var settings = EqualByFieldsSettings.GetOrCreate(referenceHandling);
            return FieldValues(x, y, settings);
        }

        /// <summary>
        /// Compares x and y for equality using field values.
        /// If a type implements IList the items of the list are compared
        /// </summary>
        /// <param name="referenceHandling">
        /// Specifies how reference types are compared.
        /// Structural compares field values recursively.
        /// </param>
        public static bool FieldValues<T>(T x, T y, BindingFlags bindingFlags, ReferenceHandling referenceHandling)
        {
            var settings = EqualByFieldsSettings.GetOrCreate(bindingFlags, referenceHandling);
            return FieldValues(x, y, settings);
        }

        public static bool FieldValues<T>(T x, T y, params string[] excludedFields)
        {
            return FieldValues(x, y, Constants.DefaultFieldBindingFlags, excludedFields);
        }

        public static bool FieldValues<T>(T x, T y, BindingFlags bindingFlags, params string[] excludedFields)
        {
            var settings = EqualByFieldsSettings.Create(x, y, bindingFlags, ReferenceHandling.Throw, excludedFields);
            return FieldValues(x, y, settings);
        }

        public static bool FieldValues<T>(T x, T y, EqualByFieldsSettings settings)
        {
            if (settings.ReferenceHandling == ReferenceHandling.Throw)
            {
                var type = x?.GetType() ?? y?.GetType() ?? typeof(T);
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    ThrowCannotCompareFieldsForType(type);
                }

                var fieldInfos = type.GetFields(settings.BindingFlags);
                foreach (var fieldInfo in fieldInfos)
                {
                    if (settings.IsIgnoringField(fieldInfo))
                    {
                        continue;
                    }

                    if (!fieldInfo.FieldType.IsEquatable())
                    {
                        ThrowCannotCompareField(type, fieldInfo);
                    }
                }
            }

            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return FieldValuesCore(x, y, settings);
        }

        private static bool FieldValuesCore(object x, object y, EqualByFieldsSettings settings)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            if (x is IEnumerable)
            {
                if (!EnumerableEquals(x, y, FieldItemEquals, settings))
                {
                    return false;
                }
            }

            var fieldInfos = x.GetType().GetFields(settings.BindingFlags);
            foreach (var fieldInfo in fieldInfos)
            {
                if (settings.IsIgnoringField(fieldInfo))
                {
                    continue;
                }

                var xv = fieldInfo.GetValue(x);
                var yv = fieldInfo.GetValue(y);

                if (!FieldValueEquals(xv, yv, fieldInfo, settings))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool FieldItemEquals(object x, object y, EqualByFieldsSettings settings)
        {
            return FieldValueEquals(x, y, null, settings);
        }

        private static bool FieldValueEquals(object x, object y, FieldInfo fieldInfo, EqualByFieldsSettings settings)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            if (IsEquatable(x.GetType()))
            {
                if (!Equals(x, y))
                {
                    return false;
                }
            }
            else
            {
                switch (settings.ReferenceHandling)
                {
                    case ReferenceHandling.Reference:
                        if (ReferenceEquals(x, y))
                        {
                            return true;
                        }

                        return false;
                    case ReferenceHandling.Structural:
                        if (FieldValuesCore(x, y, settings))
                        {
                            return true;
                        }

                        return false;
                    case ReferenceHandling.Throw:
                        ThrowCannotCompareField(x.GetType(), fieldInfo);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(settings.ReferenceHandling), settings.ReferenceHandling, null);
                }
            }

            return true;
        }

        private static void ThrowCannotCompareField(Type type, FieldInfo fieldInfo)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine($"EqualBy.{nameof(FieldValues)}(x, y) does not support comparing the field {type.PrettyName()}.{fieldInfo.Name} of type {fieldInfo.FieldType.PrettyName()}.")
                        .AppendSolveTheProblemBy()
                        .AppendSuggestImplementIEquatable(fieldInfo.FieldType)
                        .AppendSuggestEqualBySettings<EqualByFieldsSettings>();
            throw new NotSupportedException(errorBuilder.ToString());
        }

        private static void ThrowCannotCompareFieldsForType(Type type)
        {
            var errorBuilder = new StringBuilder();
            errorBuilder.AppendLine($"EqualBy.{nameof(FieldValues)}(x, y) does not support comparing the type {type.PrettyName()}.")
                        .AppendSolveTheProblemBy()
                        .AppendSuggestImplementIEquatable(type)
                        .AppendSuggestEqualBySettings<EqualByPropertiesSettings>();
            throw new NotSupportedException(errorBuilder.ToString());
        }
    }
}
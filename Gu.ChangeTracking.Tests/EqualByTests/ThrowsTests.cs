namespace Gu.ChangeTracking.Tests.EqualByTests
{
    using System;

    using NUnit.Framework;

    public abstract class ThrowsTests
    {
        public abstract bool EqualByMethod<T>(T x, T y);

        [Test]
        public void ComplexValueThrowsWithoutReferenceHandling()
        {
            var expected = this.GetType() == typeof(FieldValues.Throws)
            ? "EqualBy.FieldValues(x, y) failed.\r\n" +
            "The field WithComplexProperty.<ComplexType>k__BackingField is not supported.\r\n" +
            "The field is of type ComplexType.\r\n" +
            "Solve the problem by any of:\r\n" +
            "* Implement IEquatable<ComplexType> for ComplexType or use a type that does.\r\n" +
            "* Use EqualByFieldsSettings to specify ReferenceHandling\r\n" +
            "  - ReferenceHandling.Structural means that a deep equals is performed.\r\n" +
            "  - ReferenceHandling.References means that reference equality is used.\r\n"

            : "EqualBy.PropertyValues(x, y) failed.\r\n" +
            "The property WithComplexProperty.ComplexType is not supported.\r\n" +
            "The property is of type ComplexType.\r\n" +
            "Solve the problem by any of:\r\n" +
            "* Implement IEquatable<ComplexType> for ComplexType or use a type that does.\r\n" +
            "* Use EqualByFieldsSettings to specify ReferenceHandling\r\n" +
            "  - ReferenceHandling.Structural means that a deep equals is performed.\r\n" +
            "  - ReferenceHandling.References means that reference equality is used.\r\n";
            var exception = Assert.Throws<NotSupportedException>(() => this.EqualByMethod<EqualByTypes.WithComplexProperty>(null, null));
            Assert.AreEqual(expected, exception.Message);
        }
    }
}
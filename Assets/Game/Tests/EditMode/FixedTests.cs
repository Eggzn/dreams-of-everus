using DreamsOfEverus.Simulation;
using NUnit.Framework;

namespace DreamsOfEverus.Tests
{
    public class FixedTests
    {
        [Test]
        public void FromInt_StoresWholeNumbers()
        {
            Assert.AreEqual(3 * Fixed.OneRaw, Fixed.FromInt(3).Raw);
            Assert.AreEqual(-2 * Fixed.OneRaw, Fixed.FromInt(-2).Raw);
        }

        [Test]
        public void FromFraction_IsExact()
        {
            Assert.AreEqual(Fixed.FromInt(3) / Fixed.FromInt(2), Fixed.FromFraction(3, 2));
            Assert.AreEqual(Fixed.Half, Fixed.FromFraction(1, 2));
        }

        [Test]
        public void AddAndSubtract()
        {
            Assert.AreEqual(Fixed.FromInt(5), Fixed.FromInt(2) + Fixed.FromInt(3));
            Assert.AreEqual(Fixed.FromInt(-1), Fixed.FromInt(2) - Fixed.FromInt(3));
        }

        [Test]
        public void Multiply_HandlesFractionsAndNegatives()
        {
            Assert.AreEqual(Fixed.FromInt(6), Fixed.FromInt(2) * Fixed.FromInt(3));
            Assert.AreEqual(Fixed.FromFraction(3, 4), Fixed.FromFraction(3, 2) * Fixed.Half);
            Assert.AreEqual(Fixed.FromInt(-6), Fixed.FromInt(-2) * Fixed.FromInt(3));
        }

        [Test]
        public void Divide_HandlesFractionsAndNegatives()
        {
            Assert.AreEqual(Fixed.FromFraction(5, 2), Fixed.FromInt(5) / Fixed.FromInt(2));
            Assert.AreEqual(Fixed.FromInt(-3), Fixed.FromInt(6) / Fixed.FromInt(-2));
        }

        [Test]
        public void FromFloat_RoundTripsEditorValues()
        {
            Assert.AreEqual(Fixed.FromFraction(1, 4), Fixed.FromFloat(0.25f));
            Assert.AreEqual(-1.5f, Fixed.FromFloat(-1.5f).ToFloat(), 0.0001f);
            Assert.AreEqual(12.3f, Fixed.FromFloat(12.3f).ToFloat(), 0.0001f);
        }

        [Test]
        public void FloorToInt_RoundsDown()
        {
            Assert.AreEqual(1, Fixed.FromFraction(3, 2).FloorToInt());
            Assert.AreEqual(-2, Fixed.FromFraction(-3, 2).FloorToInt());
        }

        [Test]
        public void MoveTowards_DoesNotOvershoot()
        {
            Fixed step = Fixed.FromInt(2);
            Assert.AreEqual(Fixed.FromInt(2), Fixed.MoveTowards(Fixed.Zero, Fixed.FromInt(10), step));
            Assert.AreEqual(Fixed.FromInt(10), Fixed.MoveTowards(Fixed.FromInt(9), Fixed.FromInt(10), step));
            Assert.AreEqual(Fixed.FromInt(-2), Fixed.MoveTowards(Fixed.Zero, Fixed.FromInt(-10), step));
        }

        [Test]
        public void Comparisons()
        {
            Assert.IsTrue(Fixed.Half < Fixed.One);
            Assert.IsTrue(Fixed.FromInt(-1) < Fixed.Zero);
            Assert.AreEqual(-1, Fixed.Sign(Fixed.FromInt(-4)));
            Assert.AreEqual(Fixed.FromInt(3), Fixed.Abs(Fixed.FromInt(-3)));
            Assert.AreEqual(Fixed.One, Fixed.Clamp(Fixed.FromInt(5), Fixed.Zero, Fixed.One));
        }

        [Test]
        public void Vector_AddAndScale()
        {
            var a = new FixedVector2(Fixed.FromInt(1), Fixed.FromInt(2));
            var b = new FixedVector2(Fixed.FromInt(3), Fixed.FromInt(-1));
            Assert.AreEqual(new FixedVector2(Fixed.FromInt(4), Fixed.FromInt(1)), a + b);
            Assert.AreEqual(new FixedVector2(Fixed.FromInt(2), Fixed.FromInt(4)), a * Fixed.FromInt(2));
        }
    }
}

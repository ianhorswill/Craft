#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScalarArithmeticTests.cs" company="Ian Horswill">
// Copyright (C) 2014 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using Craft;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class ScalarArithmeticTests
    {
        [TestMethod]
        public void UnconstrainedSumTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var sum = a + b;

            for (int i = 0; i < 1000; i++)
            {
                p.NewSolution();
                Assert.IsTrue(MathUtil.NearlyEqual(sum.UniqueValue, (a.UniqueValue + b.UniqueValue)));
            }
        }

        [TestMethod]
        public void SemiconstrainedSumTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var sum = a + b;
            sum.MustEqual(1);

            for (int i = 0; i < 1000; i++)
            {
                p.NewSolution();
                Assert.IsTrue(MathUtil.NearlyEqual(sum.UniqueValue, (a.UniqueValue + b.UniqueValue)));
            }
        }

        [TestMethod]
        public void QuadraticTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -100, 100);
            var b = new FloatVariable("b", p, -100, 100);
            var quad = (a^2) + b;
            bool fail = false;
            quad.NarrowTo(new Interval(10, 20), ref fail);

            for (int i = 0; i < 1000; i++)
            {
                p.NewSolution();
                Assert.IsTrue(MathUtil.NearlyEqual(quad.UniqueValue, (a.UniqueValue * a.UniqueValue + b.UniqueValue)));
            }
        }

        [TestMethod]
        public void SumTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var sum = a + b;
            a.MustEqual(0.5f);
            b.MustEqual(0.25f);

            p.TestConsistency();
            AssertUnique(sum, 0.75f);
        }

        [TestMethod]
        public void SumATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var sum = a + b;
            b.MustEqual(0.5f);
            sum.MustEqual(1);

            p.TestConsistency();
            AssertUnique(a, 0.5f);
        }

        [TestMethod]
        public void SumBTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var sum = a + b;
            a.MustEqual(0.5f);
            sum.MustEqual(1);

            p.TestConsistency();
            AssertUnique(b, 0.5f);
        }

        [TestMethod]
        public void DifferenceTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var difference = a - b;
            a.MustEqual(0.5f);
            b.MustEqual(0.25f);

            p.TestConsistency();
            AssertUnique(difference, 0.25f);
        }

        [TestMethod]
        public void DifferenceATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var difference = a - b;
            b.MustEqual(0.5f);
            difference.MustEqual(0.5f);

            p.TestConsistency();
            AssertUnique(a, 1);
        }

        [TestMethod]
        public void DifferenceBTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var difference = a - b;
            a.MustEqual(0.5f);
            difference.MustEqual(0.25f);

            p.TestConsistency();
            AssertUnique(b, 0.25f);
        }

        [TestMethod]
        public void ProductTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var product = a * b;
            a.MustEqual(0.5f);
            b.MustEqual(0.5f);

            p.TestConsistency();
            AssertUnique(product, 0.25f);
        }

        [TestMethod]
        public void ProductATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var product = a * b;
            b.MustEqual(0.5f);
            product.MustEqual(0.5f);

            p.TestConsistency();
            AssertUnique(a, 1);
        }

        [TestMethod]
        public void ProductBTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var product = a * b;
            a.MustEqual(0.5f);
            product.MustEqual(0.25f);

            p.TestConsistency();
            AssertUnique(b, 0.5f);
        }

        [TestMethod]
        public void QuotientTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 1);
            var b = new FloatVariable("b", p, 0, 1);
            var quotient = a / b;
            a.MustEqual(0.5f);
            b.MustEqual(0.5f);

            p.TestConsistency();
            AssertUnique(quotient, 1);
        }

        [TestMethod]
        public void QuotientATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 3);
            var b = new FloatVariable("b", p, 0, 3);
            var quotient = a / b;
            b.MustEqual(0.5f);
            quotient.MustEqual(0.5f);

            p.TestConsistency();
            AssertUnique(a, 0.25f);
        }

        [TestMethod]
        public void QuotientBTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 3);
            var b = new FloatVariable("b", p, 0, 3);
            var quotient = a / b;
            a.MustEqual(0.5f);
            quotient.MustEqual(0.25f);

            p.TestConsistency();
            AssertUnique(b, 2f);
        }

        [TestMethod]
        public void OddPowerNegativeTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 3);
            var power = a^3;
            a.MustEqual(-2f);

            p.TestConsistency();
            AssertUnique(power, -8f);
        }

        [TestMethod]
        public void OddPowerNegativeATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 3);
            var power = a ^ 3;
            power.MustEqual(-8f);

            p.TestConsistency();
            AssertUnique(a, -2f);
        }

        [TestMethod]
        public void OddPowerPositiveTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 3);
            var power = a ^ 3;
            a.MustEqual(2f);

            p.TestConsistency();
            AssertUnique(power, 8f);
        }

        [TestMethod]
        public void OddPowerPositiveATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 3);
            var power = a ^ 3;
            power.MustEqual(8f);

            p.TestConsistency();
            AssertUnique(a, 2f);
        }

        [TestMethod]
        public void EvenPowerPositiveATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 3);
            var power = a ^ 2;
            power.MustEqual(4f);

            p.TestConsistency();
            AssertUnique(a, 2f);
        }

        [TestMethod]
        public void EvenPowerPositiveTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, 0, 3);
            var power = a ^ 2;
            a.MustEqual(2f);

            p.TestConsistency();
            AssertUnique(power, 4f);
        }

        [TestMethod]
        public void EvenPowerNegativeATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 0);
            var power = a ^ 2;
            power.MustEqual(4f);

            p.TestConsistency();
            AssertUnique(a, -2f);
        }

        [TestMethod]
        public void EvenPowerNegativeTest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 0);
            var power = a ^ 2;
            a.MustEqual(-2f);

            p.TestConsistency();
            AssertUnique(power, 4f);
        }

        [TestMethod]
        public void EvenPowerZeroCrossingATest()
        {
            var p = new CSP();
            var a = new FloatVariable("a", p, -3, 3);
            var power = a ^ 2;
            power.MustEqual(4f);

            p.TestConsistency();
            Assert.AreEqual(new Interval(-2,2), a.Value);
        }

        static void AssertUnique(FloatVariable v, double value)
        {
            Assert.IsTrue(v.IsUnique);
            Assert.AreEqual(value, v.UniqueValue);
        }
    }
}

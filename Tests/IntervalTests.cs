#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntervalTests.cs" company="Ian Horswill">
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
    public class IntervalTests
    {
        [TestMethod]
        public void EmptyTest()
        {
            Assert.IsTrue(new Interval(1, -1).Empty);
            Assert.IsFalse(new Interval(-1, 1).Empty);
        }

        [TestMethod]
        public void ContainsScalarTest()
        {
            Assert.IsTrue(new Interval(0,1).Contains(1));
            Assert.IsTrue(new Interval(0,1).Contains(0));
            Assert.IsTrue(new Interval(0,1).Contains(0.5f));
            Assert.IsFalse(new Interval(0, 1).Contains(-1));
            Assert.IsFalse(new Interval(0, 1).Contains(2));
        }

        [TestMethod]
        public void ContainsIntervalTest()
        {
            Assert.IsTrue(new Interval(0, 1).Contains(new Interval(0, 1)));
            Assert.IsTrue(new Interval(0, 1).Contains(new Interval(1, 1)));
            Assert.IsTrue(new Interval(0, 1).Contains(new Interval(0, 0)));
            Assert.IsTrue(new Interval(0, 1).Contains(new Interval(.25f, .75f)));

            Assert.IsFalse(new Interval(0, 1).Contains(new Interval(0, 2)));
            Assert.IsFalse(new Interval(0, 1).Contains(new Interval(1, 2)));
            Assert.IsFalse(new Interval(0, 1).Contains(new Interval(-1, 0)));
            Assert.IsFalse(new Interval(0, 1).Contains(new Interval(-.25f, .75f)));
        }

        [TestMethod]
        public void IntersectionTest()
        {
            Assert.AreEqual(new Interval(1, 2), Interval.Intersection(new Interval(1, 2), new Interval(1, 2)));
            Assert.AreEqual(new Interval(1, 2), Interval.Intersection(new Interval(1, 2), new Interval(0, 3)));
            Assert.AreEqual(new Interval(1, 2), Interval.Intersection(new Interval(0, 2), new Interval(1, 3)));
            Assert.AreEqual(new Interval(1, 2), Interval.Intersection(new Interval(1, 2), new Interval(1, 3)));
            Assert.AreEqual(new Interval(1, 2), Interval.Intersection(new Interval(1, 2), new Interval(0, 2)));
            Assert.AreEqual(new Interval(2, 2), Interval.Intersection(new Interval(1, 2), new Interval(2, 4)));
            Assert.IsTrue(Interval.Intersection(new Interval(1, 2), new Interval(3, 4)).Empty);
        }

        [TestMethod]
        public void UnionTest()
        {
            Assert.AreEqual(new Interval(1, 2), Interval.UnionBound(new Interval(1, 2), new Interval(1, 2)));
            Assert.AreEqual(new Interval(0, 3), Interval.UnionBound(new Interval(1, 2), new Interval(0, 3)));
            Assert.AreEqual(new Interval(0, 3), Interval.UnionBound(new Interval(0, 2), new Interval(1, 3)));
            Assert.AreEqual(new Interval(1, 3), Interval.UnionBound(new Interval(1, 2), new Interval(1, 3)));
            Assert.AreEqual(new Interval(0, 2), Interval.UnionBound(new Interval(1, 2), new Interval(0, 2)));
            Assert.AreEqual(new Interval(1, 4), Interval.UnionBound(new Interval(1, 2), new Interval(3, 4)));
            Assert.AreEqual(new Interval(1, 2), Interval.UnionBound(new Interval(1, 2), new Interval(3, -4)));
        }

        [TestMethod]
        public void AddIntervalTest()
        {
            Assert.AreEqual(new Interval(1, 3), new Interval(0,1) + new Interval(1, 2));
        }

        [TestMethod]
        public void SubtractIntervalTest()
        {
            Assert.AreEqual(new Interval(-2, 0), new Interval(0, 1) - new Interval(1, 2));
        }

        [TestMethod]
        public void MultiplyIntervalTest()
        {
            Assert.AreEqual(new Interval(2, 2), new Interval(1, 1) * new Interval(2, 2));
            Assert.AreEqual(new Interval(2, 6), new Interval(1, 2) * new Interval(2, 3));
            Assert.AreEqual(new Interval(-3, 6), new Interval(-1, 2) * new Interval(2, 3));
            Assert.AreEqual(new Interval(-12, 8), new Interval(-2, 3) * new Interval(-4, 1));
            Assert.AreEqual(new Interval(1, 4), new Interval(-2, -1) * new Interval(-2, -1));
        }

        [TestMethod]
        public void DivideIntervalTest()
        {
            Assert.AreEqual(Interval.AllValues, new Interval(1, 1) / new Interval(-1, 1));
            Assert.AreEqual(Interval.AllValues, new Interval(-1, 1) / new Interval(-1, 1));
            Assert.AreEqual(new Interval(1, 4), new Interval(2, 4) / new Interval(1, 2));
            Assert.AreEqual(new Interval(-4, -1), new Interval(2, 4) / new Interval(-2, -1));
            Assert.AreEqual(new Interval(1, double.PositiveInfinity), new Interval(1, 2) / new Interval(0, 1));
            Assert.AreEqual(new Interval(double.NegativeInfinity, -1), new Interval(1, 2) / new Interval(-1, 0));
        }

        [TestMethod]
        public void IntegerPowerIntervalTest()
        {
            var neg = new Interval(-2, -1);
            var pos = new Interval(1, 2);
            var cross = new Interval(-2, 2);

            Assert.AreEqual(new Interval(1, 1), neg ^ 0);
            Assert.AreEqual(neg, neg ^ 1);
            Assert.AreEqual(new Interval(1, 4), neg ^ 2);
            Assert.AreEqual(new Interval(1, 4), pos ^ 2);
            Assert.AreEqual(new Interval(0, 4), cross ^ 2);

            Assert.AreEqual(new Interval(-8, -1), neg ^ 3);
            Assert.AreEqual(new Interval(1, 8), pos ^ 3);
        }

        [TestMethod]
        public void SqrtIntervalTest()
        {
            Assert.AreEqual(new Interval(2,3), Interval.PositiveSqrt(new Interval(4,9)));
        }
    }
}

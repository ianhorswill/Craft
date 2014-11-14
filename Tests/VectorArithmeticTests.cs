#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorArithmeticTests.cs" company="Ian Horswill">
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

using System;

using Craft;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class VectorArithmeticTests
    {
        private static readonly BoundingBox box = new BoundingBox(new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1));

        [TestMethod]
        public void DotProductOneVectorFixedTest()
        {
            var p = new CSP();
            var eX = new Vector3Variable("eX", p, 1, 0, 0);
            var unknown = new Vector3Variable("unknown", p, box);
            var dot = Vector3Variable.Dot(eX, unknown);
            dot.MustEqual(0);

            for (int count = 0; count < 1000; count++)
            {
                p.NewSolution();
                Assert.IsTrue(MathUtil.NearlyEqual(unknown.X.UniqueValue, 0));
            }
        }

        [TestMethod]
        public void DotProductTest()
        {
            var p = new CSP();
            var v1 = new Vector3Variable("v1", p, box);
            var v2 = new Vector3Variable("v2", p, box);
            var dot = Vector3Variable.Dot(v1, v2);
            dot.MustEqual(0);

            for (int count = 0; count < 1000; count++)
            {
                p.NewSolution();
                double dotProduct = v1.X.UniqueValue * v2.X.UniqueValue + v1.Y.UniqueValue * v2.Y.UniqueValue + v1.Z.UniqueValue * v2.Z.UniqueValue;
                Assert.IsTrue(MathUtil.NearlyEqual(dotProduct, 0), "Dot product not zero; was: "+dotProduct);
            }
        }

        [TestMethod]
        public void UnitVectorTest()
        {
            var p = new CSP();
            var v = new Vector3Variable("v", p, box);
            v.Magnitude.MustEqual(1);

            for (int count = 0; count < 1000; count++)
            {
                p.NewSolution();
                double magnitude = Math.Sqrt(v.X*v.X+v.Y*v.Y+v.Z*v.Z);
                Assert.IsTrue(MathUtil.NearlyEqual(magnitude, 1), "Magnitude not unity; was: " + magnitude);
            }
        }

        [TestMethod]
        public void OrthonormalBasisTest()
        {
            var p = new CSP();
            p.MaxSteps = 10000000;
            var v1 = new Vector3Variable("v1", p, box);
            var v2 = new Vector3Variable("v2", p, box);
            var v3 = new Vector3Variable("v3", p, box);
            v1.Magnitude.MustEqual(1);
            v2.Magnitude.MustEqual(1);
            v3.Magnitude.MustEqual(1);
            v1.MustBePerpendicular(v2);
            v2.MustBePerpendicular(v3);
            v3.MustBePerpendicular(v1);

            for (int count = 0; count < 10; count++)
            {
                p.NewSolution();
                double magnitude = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
                Assert.IsTrue(MathUtil.NearlyEqual(magnitude, 1), "Magnitude not unity; was: " + magnitude);
                double dotProduct = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
                Assert.IsTrue(MathUtil.NearlyEqual(dotProduct, 0), "Dot product not zero; was: " + dotProduct);

                magnitude = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);
                Assert.IsTrue(MathUtil.NearlyEqual(magnitude, 1), "Magnitude not unity; was: " + magnitude);
                dotProduct = v2.X * v3.X + v2.Y * v3.Y + v2.Z * v3.Z;
                Assert.IsTrue(MathUtil.NearlyEqual(dotProduct, 0), "Dot product not zero; was: " + dotProduct);

                magnitude = Math.Sqrt(v3.X * v3.X + v3.Y * v3.Y + v3.Z * v3.Z);
                Assert.IsTrue(MathUtil.NearlyEqual(magnitude, 1), "Magnitude not unity; was: " + magnitude);
                dotProduct = v3.X * v1.X + v3.Y * v1.Y + v3.Z * v1.Z;
                Assert.IsTrue(MathUtil.NearlyEqual(dotProduct, 0), "Dot product not zero; was: " + dotProduct);
            }
        }
    }
}

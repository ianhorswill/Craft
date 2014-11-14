#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanonicalizationTests.cs" company="Ian Horswill">
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
    public class CanonicalizationTests
    {
        [TestMethod]
        public void CachingTest()
        {
            int i = 0;
            Func<int> f = () => i++;
            var t = new MemoTable();

            Assert.AreEqual(t.Memoize("x", f, 1), t.Memoize("x", f, 1), "Memoize did not return the same value for the same arguments");
            Assert.AreNotEqual(t.Memoize("x", f, 1), t.Memoize("x", f, 2), "Memoize returned the same value for the different arguments");
        }

        [TestMethod]
        public void EqualityConstraintTest()
        {
            {
                var p = new CSP();
                var a = new FloatVariable("a", p, 0, 1);
                var b = new FloatVariable("b", p, 0, 1);
                var c = new FloatVariable("c", p);
                c.MustEqual(b);
                var sum = a + b;
                sum.MustEqual(1);

                for (int i = 0; i < 10; i++)
                {
                    p.NewSolution();
                    Assert.IsTrue(MathUtil.NearlyEqual(sum.UniqueValue, (a.UniqueValue + b.UniqueValue)));
                    Assert.AreEqual(c.Value, b.Value);
                }
            }

            {
                var p = new CSP();
                var a = new FloatVariable("a", p, 0, 1);
                var b = new FloatVariable("b", p, 0, 1);
                var c = new FloatVariable("c", p);
                b.MustEqual(c);
                var sum = a + b;
                sum.MustEqual(1);

                for (int i = 0; i < 10; i++)
                {
                    p.NewSolution();
                    Assert.IsTrue(MathUtil.NearlyEqual(sum.UniqueValue, (a.UniqueValue + b.UniqueValue)));
                    Assert.AreEqual(c.Value, b.Value);
                }
            }
        }
    }
}

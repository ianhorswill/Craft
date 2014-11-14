#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestorableTests.cs" company="Ian Horswill">
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
    public class RestorableTests
    {
        [TestMethod]
        public void InitializedValue()
        {
            var s = new UndoStack<int>();
            var a = new Restorable<int>(s, 0);
            Assert.AreEqual(0, a);
        }

        [TestMethod]
        public void Setting()
        {
            var s = new UndoStack<int>();
            var a = new Restorable<int>(s, 0);
            a.Value = 1;
            Assert.AreEqual(1, a);
        }

        [TestMethod]
        public void SetAndRestore()
        {
            var s = new UndoStack<int>();
            var a = new Restorable<int>(s, 0);
            var mark1 = s.MarkStack();
            a.Value = 1;
            Assert.AreEqual(1, a);
            var mark2 = s.MarkStack();
            a.Value = 2;
            Assert.AreEqual(2, a);
            s.Restore(mark2);
            Assert.AreEqual(1, a);
            a.Value = 3;
            Assert.AreEqual(3, a);
            var mark3 = s.MarkStack();
            a.Value = 4;
            Assert.AreEqual(4, a);
            s.Restore(mark3);
            Assert.AreEqual(3, a);
            s.Restore(mark1);
            Assert.AreEqual(0, a);
        }
    }
}

#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MemoTable.cs" company="Ian Horswill">
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
using System.Collections.Generic;

namespace Craft
{
    public class MemoTable
    {
        readonly Dictionary<string, Dictionary<Tuple, object>> cache = new Dictionary<string, Dictionary<Tuple, object>>();
        private class Tuple
        {
            private readonly object[] data;

            public Tuple(object[] arguments)
            {
                data = arguments;
            }

            public override bool Equals(object obj)
            {
                var t = obj as Tuple;
                if (t == null || t.data.Length != this.data.Length)
                    return false;
// ReSharper disable LoopCanBeConvertedToQuery
                for (int i = 0; i < data.Length; i++)
// ReSharper restore LoopCanBeConvertedToQuery
                    if (!t.data[i].Equals(data[i]))
                        return false;
                return true;
            }

            public override int GetHashCode()
            {
                int hash = 0;
// ReSharper disable LoopCanBeConvertedToQuery
                foreach (var e in data)
// ReSharper restore LoopCanBeConvertedToQuery
                    hash ^= e.GetHashCode();
                return hash;
            }
        }

        public T Memoize<T>(string functionName, Func<T> function, params object[] arguments)
        {
            Dictionary<Tuple, object> table;
            var tuple = new Tuple(arguments);
            object memoizedValue;
            if (this.cache.TryGetValue(functionName, out table))
            {
                if (table.TryGetValue(tuple, out memoizedValue))
                    return (T)memoizedValue;
            }
            else
                this.cache[functionName] = table = new Dictionary<Tuple, object>();
            table[tuple] = memoizedValue = function();
            return (T)memoizedValue;
        }
    }
}

#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Restorable.cs" company="Ian Horswill">
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
using System.Diagnostics;

namespace Craft
{
    /// <summary>
    /// A value that can be saved on an undo stack and restored later.
    /// </summary>
    /// <typeparam name="T">Underlying data type to be saved</typeparam>
    [DebuggerDisplay("{Value}")]
    public class Restorable<T>
    {
        public Restorable(UndoStack<T> stack, T initialValue)
        {
            this.RealValue = initialValue;
            this.LastSaveFrame = -1;
            undoStack = stack;
        }

        internal int LastSaveFrame;
        internal T RealValue;

        private readonly UndoStack<T> undoStack;

        public static implicit operator T(Restorable<T> restorable)
        {
            return restorable.Value;
        }

        public void SetInitialValue(T value)
        {
            if (this.LastSaveFrame != -1)
                throw new InvalidOperationException("Attempted to set initial value of a restorable after it had already been saved on the undo stack.");
            this.RealValue = value;
        }

        /// <summary>
        /// Gets/sets the current value.
        /// When setting, will save the old value on the undo stack, if it hasn't already been saved in this frame.
        /// </summary>
        public T Value
        {
            get
            {
                return this.RealValue;
            }
            set
            {
                undoStack.MaybeSave(this);
                this.RealValue = value;
            }
        }
    }
}

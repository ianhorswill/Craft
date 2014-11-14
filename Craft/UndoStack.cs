#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UndoStack.cs" company="Ian Horswill">
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

namespace Craft
{
    public class UndoStack<T>
    {
        public int MarkStack()
        {
            framepointer = undoStackPointer;
            return framepointer;
        }

        public void Restore(int frame)
        {
//#if DEBUG
//            Trace.WriteLine(string.Format("Undo to {0} from {1}", frame, undoStackPointer));
//#endif
            while (undoStackPointer > frame)
            {
                undoStackPointer -= 1;
                var popped = undoDataStack[undoStackPointer];
                var v = popped.Variable;
                v.RealValue = popped.SavedValue;
                v.LastSaveFrame = popped.OldFrame;
            }
            framepointer = frame;
        }

        struct StackBlock
        {
            public readonly T SavedValue;
            public readonly int OldFrame;
            public readonly Restorable<T> Variable; 

            public StackBlock(Restorable<T> variable)
                : this()
            {
                SavedValue = variable.RealValue;
                OldFrame = variable.LastSaveFrame;
                this.Variable = variable;
            }
        }

        private const int InitialStackSize = 128;
        StackBlock[] undoDataStack = new StackBlock[InitialStackSize];
        private int undoStackPointer;  // Location where next spilled value will be stored.
        private int framepointer;

        private void EnsureSpace()
        {
            if (undoStackPointer >= undoDataStack.Length)
            {
                var newDataStack = new StackBlock[undoDataStack.Length * 2];
                Array.Copy(undoDataStack, newDataStack, undoDataStack.Length);
                undoDataStack = newDataStack;
            }
        }

        internal void MaybeSave(Restorable<T> restorable)
        {
            if (restorable.LastSaveFrame != framepointer)
            {
//#if DEBUG
//                Trace.WriteLine(string.Format("Save {0} <- {1}", undoStackPointer, restorable.RealValue));
//#endif
                EnsureSpace();
                undoDataStack[undoStackPointer] = new StackBlock(restorable);
                undoStackPointer += 1;
                restorable.LastSaveFrame = framepointer;
            }
        }
    }
}

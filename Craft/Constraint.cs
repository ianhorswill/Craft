#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constraint.cs" company="Ian Horswill">
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
namespace Craft
{
    public abstract class Constraint
    {
        protected Constraint(CSP p)
        {
            CSP = p;
            p.Constraints.Add(this);
        }

        public readonly CSP CSP;
        internal bool Queued = false;
        protected Variable NarrowedVariable;

        public void QueuePropagation(Variable narrowedVariable)
        {
            if (CSP.CurrentlyPropagating(this))
                return;
//#if DEBUG
//            Trace.WriteLine("Queue "+this);
//#endif
            if (this.Queued)
                NarrowedVariable = null;
            else
            {
                NarrowedVariable = narrowedVariable;
                CSP.QueueConstraint(this);
            }
        }

        /// <summary>
        /// NarrowedVariable has changed, so propagate changes to other variables
        /// If NarrowedVariable is null, then multiple variables have changed.
        /// </summary>
        /// <param name="fail"></param>
        internal abstract void Propagate(ref bool fail);

        /// <summary>
        /// Update any internal fields pointing to variables to point only to their canonical versions.
        /// </summary>
        public abstract void CanonicalizeVariables();

        public T RegisterCanonical<T,T2>(T variable) where T : Variable<T2>
        {
            var canonical = variable.CanonicalVariable<T>();
            canonical.AddConstraint(this);
            return canonical;
        }
    }
}

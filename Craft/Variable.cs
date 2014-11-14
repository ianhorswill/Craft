#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Variable.cs" company="Ian Horswill">
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Craft
{
    public abstract class Variable
    {
        protected Variable(string name, CSP p)
        {
            Name = name;
            p.Variables.Add(this);
            CSP = p;
        }

        public readonly CSP CSP;

        protected List<Constraint> Constraints = new List<Constraint>();
        public readonly string Name;

        internal void AddConstraint(Constraint constraint)
        {
            CSP.AssertSolvingPhase();
            Debug.Assert(IsCanonical);
            Constraints.Add(constraint);
        }

        public abstract bool IsUnique { get; }

        /// <summary>
        /// True if this is the canonical member of this variable's equivalance class.
        /// </summary>
        public abstract bool IsCanonical { get; }

        public abstract IEnumerable TryNarrowing();

        public abstract void InitializeStartingWidth();
        public abstract double RelativeMeasure { get; }
    }

    [DebuggerDisplay("{Name}: {Value}")]
    public abstract class Variable<T> : Variable
    {
        protected Restorable<T> CurrentValue;
        protected Variable(string name, CSP p, UndoStack<T> stack, T initialValue)
            : base(name, p)
        {
            CurrentValue = new Restorable<T>(stack, initialValue);
            forwardingPointer = this;
        }

        public T Value {
            get
            {
                if (forwardingPointer == this)
                    return this.CurrentValue.Value;
                return CanonicalVariable<Variable<T>>().CurrentValue.Value;
            }
        }

        /// <summary>
        /// Adds the constraint that this and v must have the same value.
        /// Constraint is implemented by forwarding this variable on to v.
        /// </summary>
        /// <param name="v">Variable to equate this to.</param>
        public virtual void MustEqual(Variable<T> v)
        {
            CSP.AssertConfigurationPhase();
            CanonicalVariable<Variable<T>>().forwardingPointer = v.CanonicalVariable<Variable<T>>();
            CurrentValue = null;  // To make sure any further attempt to use CurrentValue will fail.
        }

        private Variable<T> forwardingPointer;

        public override bool IsCanonical
        {
            get { return forwardingPointer == this; }
        }

        public TRealType CanonicalVariable<TRealType>() where TRealType : Variable<T>
        {
            Variable<T> v = this;
            while (v != v.forwardingPointer)
                v = v.forwardingPointer;
            Debug.Assert(v.IsCanonical);
            return (TRealType)v;
        }
    }
}

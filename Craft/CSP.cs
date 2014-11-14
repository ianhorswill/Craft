#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CSP.cs" company="Ian Horswill">
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
#define RandomizeVariableChoice

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Craft
{
    /// <summary>
    /// Represents a Constraint Satisfaction Problem over doubleing-point numbers.
    /// </summary>
    public class CSP
    {
        /// <summary>
        /// Makes a new Constraint Satisfaction Problem and sets it to configuration phase.
        /// </summary>
        public CSP()
        {
            ConfigurationPhase = true;
        }

        #region Instance variables
        internal readonly List<Variable> Variables = new List<Variable>();
        private readonly List<Variable> canonicalVariables = new List<Variable>(); 
        internal readonly List<Constraint> Constraints = new List<Constraint>();
        internal UndoStack<Interval> IntervalUndoStack = new UndoStack<Interval>();
        readonly Stack<string> choiceStack = new Stack<string>();
        private readonly MemoTable memoTable = new MemoTable();
        readonly List<Variable> nonUniqueVariables = new List<Variable>(); 
        private readonly Queue<Constraint> pending = new Queue<Constraint>();
        /// <summary>
        /// MAximum number of solverSteps the solver should run for before giving up.
        /// </summary>
        public int MaxSteps = 1000;
        /// <summary>
        /// Number of solverSteps the solver has run so far.
        /// </summary>
        private int solverSteps;
        #endregion

        #region Public properties
        /// <summary>
        /// The total number of constraint variables in this CSP
        /// </summary>
        public int VariableCount
        {
            get
            {
                return Variables.Count;
            }
        }

        /// <summary>
        /// The total number of constraint objects in this CSP
        /// </summary>
        public int ConstraintCount
        {
            get
            {
                return Constraints.Count;
            }
        }
        #endregion

        /// <summary>
        /// Finds a random solution to the CSP.
        /// </summary>
        public void NewSolution()
        {
#if DEBUG
            Trace.WriteLine("New solution");
#endif
            this.StartSolutionPhase();
            choiceStack.Clear();
            this.solverSteps = 0;
            IntervalUndoStack.Restore(0);
            try
            {
                this.ClearPendingQueue();
                foreach (var c in Constraints)
                    this.pending.Enqueue(c);
                bool fail = false;
                this.MakeConsistent(ref fail);
                if (fail)
                    throw new Exception("Initial configuration is unsatisfiable.");
                foreach (var v in canonicalVariables)
                    v.InitializeStartingWidth();
            
                if (!this.Solutions().GetEnumerator().MoveNext())
                    throw new Exception("No solution found");
            }
            catch (StackOverflowException e)
            {
                var b = new StringBuilder();
                foreach (var s in choiceStack)
                {
                    b.Append(s);
                    b.Append('\n');
                }
                throw new Exception(b.ToString(), e);
            }
        }

        #region Solution search
        private IEnumerable<bool> Solutions()
        {
            if (choiceStack.Count>200)
                throw new StackOverflowException();
            if (this.solverSteps++ > MaxSteps)
                throw new TimeoutException("The Craft solver ran for too many steps");

#if DEBUG
            foreach (var iv in Variables)
            {
                var i = (FloatVariable)iv;
                Debug.Assert(!i.Value.Empty, "Variable "+iv.Name+" is empty on entry to Solutions()");
            }
#endif
            Variable v = ChooseVariable();
            if (v == null)
                yield return true;
            else
            {
                var mark = IntervalUndoStack.MarkStack();
#pragma warning disable 168
                // ReSharper disable UnusedVariable
                foreach (var ignore in v.TryNarrowing())
#pragma warning restore 168
                {
                    bool fail = false;
                    this.MakeConsistent(ref fail);
                    if (!fail)
                    {
#pragma warning disable 168
                        foreach (var ignore2 in this.Solutions())
                        // ReSharper restore UnusedVariable
#pragma warning restore 168
                            yield return false;
                    }
                    this.PopChoiceStack();
                    this.IntervalUndoStack.Restore(mark);
                }
            }
        }

        [Conditional("DEBUG")]
        public void PushChoice(string format, params object[] args)
        {
            var choice = string.Format(format, args);
#if DEBUG
            Trace.WriteLine(choice);
#endif
            choiceStack.Push(choice);
           if (choiceStack.Count>10)
                Debugger.Break();
        }

        [Conditional("DEBUG")]
        public void PopChoiceStack()
        {
#if DEBUG
            Trace.WriteLine("Fail: "+choiceStack.Peek());
#endif
            choiceStack.Pop();
        }

        /// <summary>
        /// Choose a variable to narrow
        /// </summary>
        /// <returns>Variable to narrow</returns>
        Variable ChooseVariable()
        {
#if RandomizeVariableChoice
            nonUniqueVariables.Clear();
            foreach (var v in canonicalVariables)
                if (!v.IsUnique)
                    nonUniqueVariables.Add(v);
            if (nonUniqueVariables.Count > 0)
                return nonUniqueVariables[RandomInteger(0, nonUniqueVariables.Count)];
            return null;
#else
            Variable best = null;
            double maxMeasure = 0;
            foreach (var v in canonicalVariables)
            {
                double relativeMeasure = v.RelativeMeasure;
                if (relativeMeasure > maxMeasure)
                {
                    maxMeasure = relativeMeasure;
                    best = v;
                }
            }
            return best;
#endif
        }
        #endregion

        #region Constraint propagation
        public void TestConsistency()
        {
            this.StartSolutionPhase();
            this.pending.Clear();
            foreach (var c in Constraints)
                this.pending.Enqueue(c);
            bool fail = false;
            this.MakeConsistent(ref fail);
            if (fail)
                throw new Exception("No solution");
        }

        public void MakeConsistent(ref bool fail)
        {
            while (this.pending.Count > 0)
            {
                Constraint constraint = this.pending.Dequeue();
                constraint.Queued = false;
                currentConstraint = constraint;
//#if DEBUG
//                Trace.WriteLine("Propagate "+constraint);
//#endif
                constraint.Propagate(ref fail);
                currentConstraint = null;
                if (fail)
                {
                    ClearPendingQueue();
                    return;
                }
            }
        }

        private Constraint currentConstraint;

        public bool CurrentlyPropagating(Constraint c)
        {
            return currentConstraint == c;
        }

        public void QueueConstraint(Constraint c)
        {
            this.pending.Enqueue(c);
        }

        void ClearPendingQueue()
        {
            currentConstraint = null;
            while (this.pending.Count > 0)
                this.pending.Dequeue().Queued = false;
        }
        #endregion

        #region Phase (configuration/solving) tracking
        /// <summary>
        /// True if we have not yet started solving the CSP.
        /// </summary>
        public bool ConfigurationPhase { get; private set; }

        void StartSolutionPhase()
        {
            if (ConfigurationPhase)
            {
                ConfigurationPhase = false;
                foreach (var c in Constraints)
                    c.CanonicalizeVariables();
                foreach (var v in Variables)
                    if (v.IsCanonical)
                        canonicalVariables.Add(v);
            }
        }

        /// <summary>
        /// Throw an exception if we are not in the configuration phase.
        /// </summary>
        public void AssertConfigurationPhase()
        {
            if (!this.ConfigurationPhase)
                throw new InvalidOperationException("Operation can only be performed before solving.");
        }

        /// <summary>
        /// Throw an exception if we are not in the solving phase.
        /// </summary>
        public void AssertSolvingPhase()
        {
            if (this.ConfigurationPhase)
                throw new InvalidOperationException("Operation can only be performed before solving.");
        }
        #endregion

        /// <summary>
        /// Memoize function in the CSP's memo table
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="functionName">String name of function</param>
        /// <param name="function">Delegate implementing it</param>
        /// <param name="arguments">Values of arguments for function</param>
        /// <returns>Value of function</returns>
        public T Memoize<T>(string functionName, Func<T> function, params object[] arguments)
        {
            return this.memoTable.Memoize(functionName, function, arguments);
        }

        #region Random number driver
        /// <summary>
        /// Random number generator used by this package
        /// </summary>
        internal static readonly Random Random = new Random();

        /// <summary>
        /// Return a randomly chosen integer in the interval [low, high)
        /// </summary>
        /// <param name="low">Lowest value to return</param>
        /// <param name="high">One plus highest value to return</param>
        /// <returns>Value</returns>
        internal static int RandomInteger(int low, int high)
        {
            return (Random.Next() % (high - low)) + low;
        }
        #endregion
    }
}

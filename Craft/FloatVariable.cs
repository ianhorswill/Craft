#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FloatVariable.cs" company="Ian Horswill">
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
using System.Collections;
using System.Diagnostics;

namespace Craft
{
    [DebuggerDisplay("{DebugName}")]
    public class FloatVariable : Variable<Interval>
    {
        public FloatVariable(string name, CSP p, double lower, double upper) 
            : this(name, p, new Interval(lower, upper))
        {}

        public FloatVariable(string name, CSP p, Interval initialValue)
            : base(name, p, p.IntervalUndoStack, initialValue)
        {}

        public FloatVariable(string name, CSP csp)
            : this(name, csp, Interval.AllValues)
        { }

        private double startingWidth;

        public override void InitializeStartingWidth()
        {
            startingWidth = Value.Width;
        }

        public override double RelativeMeasure
        {
            get
            {
                return Value.Width / startingWidth;
            }
        }

        public static FloatVariable Constant(CSP p, double c)
        {
// ReSharper disable SpecifyACultureInStringConversionExplicitly
            return p.Memoize("constant", () => new FloatVariable(c.ToString(), p, c, c), c);
// ReSharper restore SpecifyACultureInStringConversionExplicitly
        }

        // This is needed to prevent the compiler from trying to do an implicit conversion
        // of v to a double and then taking the MustEqual(double) overload.
        public void MustEqual(FloatVariable v)
        {
            MustEqual((Variable<Interval>)v);
        }

        public override void MustEqual(Variable<Interval> v)
        {
            var iv = (FloatVariable)v;
            iv.MustBeContainedIn(Value);
            base.MustEqual(v);
        }

        public void MustBeContainedIn(Interval i)
        {
            CSP.AssertConfigurationPhase();
            Interval intersection = Interval.Intersection(Value, i);
            if (intersection.Empty)
                throw new ArgumentException("Argument out of current range of variable.");
            CurrentValue.SetInitialValue(intersection);
        }

        public void MustEqual(double uniqueValue)
        {
            this.MustBeContainedIn(new Interval(uniqueValue));
        }

        public void NarrowTo(Interval restriction, ref bool fail)
        {
            Debug.Assert(IsCanonical);
            if (Value.IsUnique)
            {
                if (restriction.NearlyContains(this.Value, MathUtil.DefaultEpsilon))
                    return;
                fail = true;
#if DEBUG
                Trace.WriteLine(string.Format("{0}: {1} -> Empty       {2}", Name, Value, restriction));
#endif
                return;
            }
#if DEBUG
            var oldValue = Value;
#endif
            if (!restriction.Contains(this.Value))
            {
                var newValue = Interval.Intersection(this.CurrentValue.Value, restriction);
                if (newValue.NearlyUnique)
                    newValue = new Interval(newValue.Midpoint);
#if SearchHints
                newValue.SearchHint = restriction.SearchHint;
#endif
#if DEBUG
                Trace.WriteLine(string.Format("{0}: {1} -> {2}       {3}, narrowed by {4}%", Name, oldValue, newValue, restriction, 100*(1-newValue.Width/oldValue.Width)));
#endif
                if (newValue.Empty)
                    fail = true;
                else
                {
                    bool propagate = newValue.Width / Value.Width < 0.99;
                    this.CurrentValue.Value = newValue;
                    if (propagate)
                        foreach (var c in Constraints)
                            c.QueuePropagation(this);
                }
            }
        }

        public void NarrowToUnion(Interval a, Interval b, ref bool fail)
        {
            this.NarrowTo(Interval.UnionOfIntersections(Value, a, b), ref fail);
        }

        public void NarrowToQuotient(Interval numerator, Interval denominator, ref bool fail)
        {
            if (denominator.IsZero)
            {
                // Denominator is [0,0], so quotient is the empty set
                fail = !numerator.ContainsZero;
                return;
            }

            if (numerator.IsZero)
            {
                if (!denominator.ContainsZero)
                    // Quotient is [0,0].
                    this.NarrowTo(new Interval(0,0), ref fail);
                // Denominator contains zero so quotient can be any value.
                return;
            }

            if (!denominator.ContainsZero)
            {
                this.NarrowTo(numerator*denominator.Reciprocal, ref fail);
                return;
            }

            // Denominator contains zero, so there are three cases: crossing zero, [0, b], and [a, 0]

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (denominator.Lower == 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                // Non-negative denominator
                if (numerator.Upper <= 0)
                {
                    this.NarrowTo(new Interval(double.NegativeInfinity, numerator.Upper / denominator.Upper), ref fail);
                    return;
                }

                if (numerator.Lower >= 0)
                {
                    this.NarrowTo(new Interval(numerator.Lower / denominator.Upper, double.PositiveInfinity), ref fail);
                    return;
                }
                // Numerator crosses zero, so quotient is all the Reals, so can't narrow interval.
                return;
            }

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (denominator.Upper == 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
            {
                // Non-positive denominator
                if (numerator.Upper <= 0)
                {
                    this.NarrowTo(new Interval(numerator.Upper / denominator.Lower, double.PositiveInfinity), ref fail);
                    return;
                }

                if (numerator.Lower >= 0)
                {
                    this.NarrowTo(new Interval(double.NegativeInfinity, numerator.Lower / denominator.Lower), ref fail);
                    return;
                }
                // Numerator crosses zero, so quotient is all the Reals, so can't narrow interval.
                return;
            }

            if (numerator.Upper < 0)
            {
                // Strictly negative
                var lowerHalf = new Interval(double.NegativeInfinity, numerator.Upper / denominator.Upper);
                var upperHalf = new Interval(numerator.Upper / denominator.Lower, double.PositiveInfinity);
                this.NarrowToUnion(lowerHalf, upperHalf, ref fail);
                return;
            }

            // Denominator crosses zero
            if (numerator.Lower > 0)
            {
                // Strictly positive
                var lowerHalf = new Interval(double.NegativeInfinity, numerator.Lower / denominator.Lower);
                var upperHalf = new Interval(numerator.Lower / denominator.Upper, double.PositiveInfinity);

                this.NarrowToUnion(lowerHalf, upperHalf, ref fail);
// ReSharper disable RedundantJumpStatement
                return;
// ReSharper restore RedundantJumpStatement
            }

            // Numerator contains zero, so quotient is all the Reals, so can't narrow interval.

        }

        public void NarrowToSignedSqrt(Interval square, ref bool fail)
        {
            var lower = Math.Max(0, square.Lower);
            var upper = square.Upper;
            if (upper < 0)
            {
                fail = true;
                return;
            }
            var sqrt = new Interval(Math.Sqrt(lower), Math.Sqrt(upper));
            Interval restriction;
            if (Value.CrossesZero)
                restriction = Interval.UnionOfIntersections(Value, sqrt, -sqrt);
            else if (Value.Upper <= 0)
                // Current value is strictly negative
                restriction = -sqrt;
            else
                // Current value is strictly positive
                restriction = sqrt;
            this.NarrowTo(restriction, ref fail);
        }

#if SearchHints
        public bool SafeToGuess
        {
            get
            {
                return Value.SearchHint != SearchHint.NoGuess;
            }
        }
#endif

        public override IEnumerable TryNarrowing()
        {
            bool fail = false;

#if SearchHints
            if (SafeToGuess)
#endif
            {
                double randomElement = Value.RandomElement();
                CSP.PushChoice("Guess {0}={1}", this.Name, randomElement);
                this.NarrowTo(new Interval(randomElement), ref fail);
                Debug.Assert(!fail, "trial narrowing failed");
                yield return false;
            }

            if ((CSP.Random.Next()&1)==0)
            {
                CSP.PushChoice("Lower half {0} to {1}", this.Name, this.Value.LowerHalf);
                this.NarrowTo(Value.LowerHalf, ref fail);
                Debug.Assert(!fail, "trial narrowing failed");
                yield return false;

                CSP.PushChoice("Upper half {0} to {1}", this.Name, this.Value.UpperHalf);
                this.NarrowTo(Value.UpperHalf, ref fail);
                Debug.Assert(!fail, "trial narrowing failed");
                yield return false;
            }
            else
            {
                CSP.PushChoice("Upper half {0} to {1}", this.Name, this.Value.UpperHalf);
                this.NarrowTo(Value.UpperHalf, ref fail);
                Debug.Assert(!fail, "trial narrowing failed");
                yield return false;

                CSP.PushChoice("Lower half {0} to {1}", this.Name, this.Value.LowerHalf);
                this.NarrowTo(Value.LowerHalf, ref fail);
                Debug.Assert(!fail, "trial narrowing failed");
                yield return false;
            }
        }
        
        public override bool IsUnique
        {
            get
            {
                return Value.IsUnique;
            }
        }

        public double UniqueValue
        {
            get
            {
                return Value.UniqueValue;
            }
        }

        public static implicit operator double(FloatVariable v)
        {
            return v.UniqueValue;
        }

        public static FloatVariable operator +(FloatVariable a, FloatVariable b)
        {
            return a.CSP.Memoize(
                "+",
                () =>
                    {
                        var sum = new FloatVariable("sum", a.CSP, a.Value + b.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new SumConstraint(sum, a, b);
                        // ReSharper restore ObjectCreationAsStatement
                        return sum;
                    },
                a,
                b);
        }

        public static FloatVariable operator -(FloatVariable a, FloatVariable b)
        {
            return a.CSP.Memoize(
                "-",
                () =>
                    {
                        var difference = new FloatVariable("difference", a.CSP, a.Value - b.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new DifferenceConstraint(difference, a, b);
                        // ReSharper restore ObjectCreationAsStatement
                        return difference;
                    },
                a,
                b);
        }

        public static FloatVariable operator *(FloatVariable a, FloatVariable b)
        {
            return a.CSP.Memoize(
                "*",
                () =>
                    {
                        var product = new FloatVariable("product", a.CSP, a.Value * b.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new ProductConstraint(product, a, b);
                        // ReSharper restore ObjectCreationAsStatement
                        return product;
                    },
                a,
                b);
        }

        public static FloatVariable operator *(double k, FloatVariable a)
        {
            return a.CSP.Memoize(
                "*",
                () =>
                    {
                        var product = new FloatVariable("product", a.CSP, k * a.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new ProductConstantConstraint(product, a, k);
                        // ReSharper restore ObjectCreationAsStatement
                        return product;
                    },
                k,
                a);
        }

        public static FloatVariable operator /(FloatVariable a, FloatVariable b)
        {
            return a.CSP.Memoize(
                "/",
                () =>
                    {
                        var quotient = new FloatVariable("quotient", a.CSP, a.Value / b.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new QuotientConstraint(quotient, a, b);
                        // ReSharper restore ObjectCreationAsStatement
                        return quotient;
                    },
                a,
                b);
        }

        public static FloatVariable operator ^(FloatVariable a, uint exponent)
        {
            return a.CSP.Memoize(
                "^",
                () =>
                    {
                        var power = new FloatVariable("power", a.CSP, a.Value ^ exponent);
                        // ReSharper disable ObjectCreationAsStatement
                        new PowerConstraint(power, a, exponent);
                        // ReSharper restore ObjectCreationAsStatement
                        return power;
                    },
                a,
                exponent);
        }
    
        internal string DebugName
        {
            get
            {
// ReSharper disable RedundantCast
                return string.Format("{0}: {1}", Name, IsUnique ? (object)UniqueValue : (object)Value);
// ReSharper restore RedundantCast
            }
        }
    }
}

#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScalarArithmeticConstraints.cs" company="Ian Horswill">
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

using System.Diagnostics;

namespace Craft
{
    [DebuggerDisplay("{sum.Name}={a.Name}+{b.Name}")]
    internal class SumConstraint : Constraint
    {
        public SumConstraint(FloatVariable sum, FloatVariable a, FloatVariable b) :
            base(sum.CSP)
        {
            this.sum = sum;
            this.a = a;
            this.b = b;
        }

        private FloatVariable sum, a, b;

        public override void CanonicalizeVariables()
        {
            sum = RegisterCanonical<FloatVariable, Interval>(sum);
            a = RegisterCanonical<FloatVariable, Interval>(a);
            b = RegisterCanonical<FloatVariable, Interval>(b); 
        }

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.sum)
            {
                this.sum.NarrowTo(this.a.Value + this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a)
            {
                this.a.NarrowTo(this.sum.Value - this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b)
                this.b.NarrowTo(this.sum.Value - this.a.Value, ref fail);
        }
    }

    [DebuggerDisplay("{difference.Name}={a.Name}-{b.Name}")]
    internal class DifferenceConstraint : Constraint
    {
        public DifferenceConstraint(FloatVariable difference, FloatVariable a, FloatVariable b) :
            base(difference.CSP)
        {
            this.difference = difference;
            this.a = a;
            this.b = b;
        }

        private FloatVariable difference;

        private FloatVariable a;

        private FloatVariable b;

        public override void CanonicalizeVariables()
        {
            difference = RegisterCanonical<FloatVariable, Interval>(difference);
            a = RegisterCanonical<FloatVariable, Interval>(a);
            b = RegisterCanonical<FloatVariable, Interval>(b); 
        }

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.difference)
            {
                this.difference.NarrowTo(this.a.Value - this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a)
            {
                this.a.NarrowTo(this.difference.Value + this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b)
                this.b.NarrowTo(this.a.Value - this.difference.Value, ref fail);
        }
    }

    [DebuggerDisplay("{product.Name}={a.Name}*{b.Name}")]
    internal class ProductConstraint : Constraint
    {
        public ProductConstraint(FloatVariable product, FloatVariable a, FloatVariable b) :
            base(product.CSP)
        {
            this.product = product;
            this.a = a;
            this.b = b;
        }

        private FloatVariable product;

        private FloatVariable a;

        private FloatVariable b;

        public override void CanonicalizeVariables()
        {
            product = RegisterCanonical<FloatVariable, Interval>(product);
            a = RegisterCanonical<FloatVariable, Interval>(a);
            b = RegisterCanonical<FloatVariable, Interval>(b); 
        }

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.product)
            {
                this.product.NarrowTo(this.a.Value * this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a)
            {
                this.a.NarrowToQuotient(this.product.Value, this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b)
                this.b.NarrowToQuotient(this.product.Value, this.a.Value, ref fail);
        }
    }

    [DebuggerDisplay("{product.Name}={a.Name}*{k}")]
    internal class ProductConstantConstraint : Constraint
    {
        public ProductConstantConstraint(FloatVariable product, FloatVariable a, double k) :
            base(product.CSP)
        {
            this.product = product;
            this.a = a;
            this.k = k;
        }

        private FloatVariable product;

        private FloatVariable a;

        public override void CanonicalizeVariables()
        {
            product = RegisterCanonical<FloatVariable, Interval>(product);
            a = RegisterCanonical<FloatVariable, Interval>(a);            
        }

        private readonly double k;

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.product)
            {
                this.product.NarrowTo(this.a.Value * k, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a)
            {
                this.a.NarrowTo(product.Value*(1/k), ref fail);
            }
        }
    }

    [DebuggerDisplay("{quotient.Name}={a.Name}/{b.Name}")]
    internal class QuotientConstraint : Constraint
    {
        public QuotientConstraint(FloatVariable quotient, FloatVariable a, FloatVariable b) :
            base(quotient.CSP)
        {
            this.quotient = quotient;
            this.a = a;
            this.b = b;
        }

        private FloatVariable quotient;

        private FloatVariable a;

        private FloatVariable b;

        public override void CanonicalizeVariables()
        {
            quotient = RegisterCanonical<FloatVariable, Interval>(quotient);
            a = RegisterCanonical<FloatVariable, Interval>(a);
            b = RegisterCanonical<FloatVariable, Interval>(b); 
        }

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.quotient)
            {
                this.quotient.NarrowToQuotient(this.a.Value, this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a)
            {
                this.a.NarrowTo(this.quotient.Value * this.b.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b)
                this.b.NarrowToQuotient(this.a.Value, this.quotient.Value, ref fail);
        }
    }

    [DebuggerDisplay("{power.Name}={a.Name}^{exponent}")]
    internal class PowerConstraint : Constraint
    {
        public PowerConstraint(FloatVariable power, FloatVariable a, uint exponent) :
            base(power.CSP)
        {
            this.power = power;
            this.a = a;
            this.exponent = exponent;
        }

        private FloatVariable power;

        private FloatVariable a;

        public override void CanonicalizeVariables()
        {
            power = RegisterCanonical<FloatVariable, Interval>(power);
            a = RegisterCanonical<FloatVariable, Interval>(a);
        }

        private readonly uint exponent;

        internal override void Propagate(ref bool fail)
        {
            if (NarrowedVariable != this.power)
            {
                this.power.NarrowTo(this.a.Value^this.exponent, ref fail);
                if (fail) return;
            }

            // We want to repropagate in case this is an even power and we just split on a.
            //if (NarrowedVariable != this.a)
            {
                if ((exponent % 2 == 0) && this.a.Value.Lower < 0)
                {
                    if (this.a.Value.Upper <= 0)
                        // a is non-positive
                        this.a.NarrowTo(-Interval.InvPower(power.Value, exponent), ref fail);
                    else
                    {
                        // even inverse power of an interval that crosses zero
                        var bound = Interval.InvPower(power.Value, exponent).Upper;
                        this.a.NarrowTo(new Interval(-bound, bound)
#if SearchHints
                        { SearchHint =  SearchHint.NoGuess }
#endif
                        , ref fail);
                    }
                }
                else
                    // a is already non-negative or exponent is odd (and so function is monotone)
                    this.a.NarrowTo(Interval.InvPower(this.power.Value, this.exponent), ref fail);
            }
        }
    }
}

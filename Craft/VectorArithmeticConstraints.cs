#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorArithmeticConstraints.cs" company="Ian Horswill">
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
    [DebuggerDisplay("{DebugName}")]
    internal class DotProductConstraint : Constraint
    {
        public DotProductConstraint(FloatVariable product, Vector3Variable a, Vector3Variable b)
            : base(product.CSP)
        {
            this.product = product;
            this.a = a;
            this.b = b;
        }

        private FloatVariable product;

        public override void CanonicalizeVariables()
        {
            product = RegisterCanonical<FloatVariable, Interval>(product);
            a.CanonicalizeAndRegisterConstraint(this);
            b.CanonicalizeAndRegisterConstraint(this);
        }

        private readonly Vector3Variable a, b;

        internal override void Propagate(ref bool fail)
        {
            Interval pX = a.X.Value * b.X.Value;
            Interval pY = a.Y.Value * b.Y.Value;
            Interval pZ = a.Z.Value * b.Z.Value;
            if (NarrowedVariable != this.product)
            {
                this.product.NarrowTo(pX + pY + pZ, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a.X)
            {
                this.a.X.NarrowToQuotient(this.product.Value - (pY + pZ), this.b.X.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b.X)
            {
                this.b.X.NarrowToQuotient(this.product.Value - (pY + pZ), this.a.X.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a.Y)
            {
                this.a.Y.NarrowToQuotient(this.product.Value - (pX + pZ), this.b.Y.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b.Y)
            {
                this.b.Y.NarrowToQuotient(this.product.Value - (pX + pZ), this.a.Y.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.a.Z)
            {
                this.a.Z.NarrowToQuotient(this.product.Value - (pX + pY), this.b.Z.Value, ref fail);
                if (fail) return;
            }

            if (NarrowedVariable != this.b.Z)
            {
                this.b.Z.NarrowToQuotient(this.product.Value - (pX + pY), this.a.Z.Value, ref fail);
            }
        }

        public override string ToString()
        {
            return string.Format("<{0} = {1} dot {2}>", product.Name, a.Name, b.Name);
        }

        internal string DebugName
        {
            get
            {
                return this.ToString(); 
            }
        }
    }

    //[DebuggerDisplay("{DebugName}")]
    //internal class CrossProductConstraint : Constraint
    //{
    //    public CrossProductConstraint(Vector3Variable product, Vector3Variable a, Vector3Variable b)
    //        : base(product.CSP)
    //    {
    //        this.product = product;
    //        this.a = a;
    //        this.b = b;
    //    }

    //    public override void CanonicalizeVariables()
    //    {
    //        product.CanonicalizeAndRegisterConstraint(this);
    //        a.CanonicalizeAndRegisterConstraint(this);
    //        b.CanonicalizeAndRegisterConstraint(this);
    //    }

    //    private readonly Vector3Variable product, a, b;

    //    internal override void Propagate(ref bool fail)
    //    {
    //        if (NarrowedVariable != this.product.X)
    //        {
    //            this.product.X.NarrowTo(a.Y.Value*b.Z.Value-a.Z.Value*b.Y.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.product.Y)
    //        {
    //            this.product.Y.NarrowTo(a.Z.Value * b.X.Value - a.X.Value * b.Z.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.product.Z)
    //        {
    //            this.product.Z.NarrowTo(a.X.Value * b.Y.Value - a.Y.Value * b.X.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.a.X)
    //        {
    //            this.a.X.NarrowToQuotient(this.product.Value - (pY + pZ), this.b.X.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.b.X)
    //        {
    //            this.b.X.NarrowToQuotient(this.product.Value - (pY + pZ), this.a.X.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.a.Y)
    //        {
    //            this.a.Y.NarrowToQuotient(this.product.Value - (pX + pZ), this.b.Y.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.b.Y)
    //        {
    //            this.b.Y.NarrowToQuotient(this.product.Value - (pX + pZ), this.a.Y.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.a.Z)
    //        {
    //            this.a.Z.NarrowToQuotient(this.product.Value - (pX + pY), this.b.Z.Value, ref fail);
    //            if (fail) return;
    //        }

    //        if (NarrowedVariable != this.b.Z)
    //        {
    //            this.b.Z.NarrowToQuotient(this.product.Value - (pX + pY), this.a.Z.Value, ref fail);
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("<{0} = {1} dot {2}>", product.Name, a.Name, b.Name);
    //    }

    //    internal string DebugName
    //    {
    //        get
    //        {
    //            return this.ToString();
    //        }
    //    }
    //}

    [DebuggerDisplay("{DebugName}")]
    internal class MagnitudeConstraint : Constraint
    {
        public MagnitudeConstraint(FloatVariable magnitude, Vector3Variable vector)
            : base(magnitude.CSP)
        {
            this.magnitude = magnitude;
            this.vector = vector;
        }

        private FloatVariable magnitude;

        public override void CanonicalizeVariables()
        {
            this.magnitude = RegisterCanonical<FloatVariable, Interval>(this.magnitude);
            this.vector.CanonicalizeAndRegisterConstraint(this);
        }

        private readonly Vector3Variable vector;

        internal override void Propagate(ref bool fail)
        {
            Interval sX = this.vector.X.Value.Square;
            Interval sY = this.vector.Y.Value.Square;
            Interval sZ = this.vector.Z.Value.Square;

            if (NarrowedVariable != this.magnitude)
            {
                this.magnitude.NarrowTo(Interval.PositiveSqrt(sX + sY + sZ), ref fail);
                if (fail) return;
            }

            var sM = this.magnitude.Value.Square;

            //if (NarrowedVariable != this.vector.X)
            {
                this.vector.X.NarrowToSignedSqrt(sM - (sY + sZ), ref fail);
                if (fail) return;
            }

            //if (NarrowedVariable != this.vector.Y)
            {
                this.vector.Y.NarrowToSignedSqrt(sM - (sX + sZ), ref fail);
                if (fail) return;
            }

            //if (NarrowedVariable != this.vector.Z)
            {
                this.vector.Z.NarrowToSignedSqrt(sM - (sX + sY), ref fail);
            }
        }

        public override string ToString()
        {
            return string.Format("<{0} = Magnitude({1})>", this.magnitude.Name, this.vector.Name);
        }

        internal string DebugName
        {
            get
            {
                return this.ToString();
            }
        }
    }
}
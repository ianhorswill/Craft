#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Interval.cs" company="Ian Horswill">
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
using System.Globalization;

namespace Craft
{
    [DebuggerDisplay("{DebugString}")]
    public struct Interval
    {
        public readonly double Lower;
        public readonly double Upper;
#if SearchHints
        public SearchHint SearchHint;
#endif

        public static Interval FromUnsortedBounds(double a, double b)
        {
            if (a > b)
                return new Interval(b, a);
            return new Interval(a, b);
        }

        public Interval(double lowerBound, double upperBound)
        {
            Debug.Assert(!double.IsNaN(lowerBound), "Interval lower bound is not a number");
            Debug.Assert(!double.IsNaN(upperBound), "Interval upper bound is not a number");
            Debug.Assert(!double.IsPositiveInfinity(lowerBound), "Interval lower bound cannot be positive infinity");
            Debug.Assert(!double.IsNegativeInfinity(upperBound), "Interval lower bound cannot be negative infinity");
            Lower = lowerBound;
            Upper = upperBound;
#if SearchHints
            SearchHint = SearchHint.None;
#endif
        }

        public Interval(double singleton)
            : this(singleton, singleton)
        {
        }

        public static readonly Interval AllValues = new Interval(double.NegativeInfinity, double.PositiveInfinity);

        public bool IsUnique
        {
            get
            {
                //return MathUtil.NearlyEqual(Upper, Lower);
// ReSharper disable CompareOfFloatsByEqualityOperator
                return Upper == Lower;
// ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

        public double UniqueValue
        {
            get
            {
                if (!IsUnique)
                    throw new InvalidOperationException("Variable value is not unique");
                return Midpoint;
            }
        }

        public bool Empty
        {
            get
            {
                return this.Lower > this.Upper;
            }
        }

        public bool Contains(double value)
        {
            return Lower <= value && value <= Upper;
        }

        public bool ContainsZero
        {
            get
            {
                return Lower <= 0 && Upper >= 0;
            }
        }

        public bool CrossesZero
        {
            get
            {
                return Lower < 0 && Upper > 0;
            }
        }

        public bool NonNegative
        {
            get
            {
                return Lower >= 0;
            }
        }

        public bool NonPositive
        {
            get
            {
                return Upper <= 0;
            }
        }

        public bool StrictlyNegative
        {
            get
            {
                return Upper < 0;
            }
        }

        public bool StrictlyPositive
        {
            get
            {
                return Lower > 0; 
            }
        }

        public bool IsZero
        {
            get
            {
// ReSharper disable CompareOfFloatsByEqualityOperator
                return Lower == 0 && Upper == 0;
// ReSharper restore CompareOfFloatsByEqualityOperator
            }
        }

        public bool Contains(Interval i)
        {
            return this.Lower <= i.Lower && this.Upper >= i.Upper;
        }

        public bool NearlyContains(Interval i, double epsilon)
        {
            return MathUtil.NearlyLE(this.Lower, i.Lower, epsilon) && MathUtil.NearlyGE(this.Upper, i.Upper, epsilon);
        }

        private const double MaxPracticalDouble = double.MaxValue * 0.5;
        private const double MinPracticalDouble = double.MinValue * 0.5;
        public double RandomElement()
        {
            double realLower = this.PracticalLower;
            double range = (this.PracticalUpper - realLower);
            Debug.Assert(!double.IsNaN(range));
            Debug.Assert(!double.IsPositiveInfinity(range));
            double randomElement = realLower + (CSP.Random.NextDouble() * range);
            Debug.Assert(!double.IsPositiveInfinity(randomElement));
            Debug.Assert(!double.IsNegativeInfinity(randomElement));
            return randomElement;
        }

        public double Width
        {
            get
            {
                return Upper - Lower;
            }
        }

        public double Abs
        {
            get
            {
                return Math.Max(Math.Abs(Lower), Math.Abs(Upper));
            }
        }

        private double PracticalUpper
        {
            get
            {
                return Math.Min(this.Upper, MaxPracticalDouble);
            }
        }

        private double PracticalLower
        {
            get
            {
                return Math.Max(this.Lower, MinPracticalDouble);
            }
        }

        public double Midpoint
        {
            get
            {
                return (PracticalLower + PracticalUpper) * 0.5f;
            }
        }
        
        public Interval LowerHalf
        {
            get
            {
                return new Interval(Lower, Midpoint);
            }
        }

        public Interval UpperHalf
        {
            get
            {
                return new Interval(Midpoint, Upper);
            }
        }

        public static Interval Intersection(Interval a, Interval b)
        {
            return new Interval(Math.Max(a.Lower, b.Lower), Math.Min(a.Upper, b.Upper));
        }

        public static Interval UnionBound(Interval a, Interval b)
        {
            if (a.Empty)
                return b;
            if (b.Empty)
                return a;
            return new Interval(Math.Min(a.Lower, b.Lower), Math.Max(a.Upper, b.Upper));
        }

        public static Interval UnionOfIntersections(Interval intersector, Interval a, Interval b)
        {
            return UnionBound(Intersection(intersector, a), Intersection(intersector, b));
        }

        public static Interval operator +(Interval a, Interval b)
        {
            // This propagate infinite shouldn't be necessary for addition because
            // we should never have infinities of the two intervals with opposite signs.
            //
            //return new Interval(
            //    PropagateNegativeInfinity(a.Lower, b.Lower, a.Lower + b.Lower),
            //    PropagatePositiveInfinity(a.Upper, b.Upper, a.Upper + b.Upper));
            return new Interval(a.Lower + b.Lower, a.Upper + b.Upper);
        }

        public static Interval operator -(Interval a, Interval b)
        {
            return new Interval(
                PropagateNegativeInfinity(a.Lower, a.Lower - b.Upper),
                PropagatePositiveInfinity(a.Upper, a.Upper - b.Lower));
        }

        public static Interval operator -(Interval a)
        {
            return new Interval(-a.Upper, -a.Lower);
        }

        public static Interval operator *(Interval a, Interval b)
        {
            return new Interval(
                Min(a.Lower * b.Lower, a.Upper * b.Upper, a.Lower * b.Upper, a.Upper * b.Lower),
                Max(a.Lower * b.Lower, a.Upper * b.Upper, a.Lower * b.Upper, a.Upper * b.Lower));
        }

        public static Interval operator *(Interval a, double k)
        {
            return new Interval(
                Math.Min(a.Lower * k, a.Upper * k),
                Math.Max(a.Lower * k, a.Upper * k));
        }

        public static Interval operator *(double k, Interval a)
        {
            return a*k;
        }

        public static Interval operator /(Interval a, Interval b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (b.Lower == 0)
            {
                if (b.Upper == 0)
                    return AllValues;
                return new Interval(Math.Min(a.Upper / b.Upper, a.Lower / b.Upper), double.PositiveInfinity);
            }

            if (b.Upper == 0)
                // ReSharper restore CompareOfFloatsByEqualityOperator
                return new Interval(double.NegativeInfinity, Math.Max(a.Lower / b.Lower, a.Upper / b.Lower));

            if (b.Contains(0))
                return AllValues;

            return new Interval(
                Min(a.Lower / b.Lower, a.Upper / b.Upper, a.Lower / b.Upper, a.Upper / b.Lower),
                Max(a.Lower / b.Lower, a.Upper / b.Upper, a.Lower / b.Upper, a.Upper / b.Lower));
        }

        public Interval Reciprocal
        {
            get
            {
                return new Interval(1 / Upper, 1 / Lower);
            }
        }

        public static Interval operator ^(Interval a, uint exponent)
        {
            switch (exponent)
            {
                case 0:
                    return new Interval(1, 1);

                case 1:
                    return a;

                default:
                    if (exponent % 2 == 0)
                    {
                        // even exponent
                        if (a.Lower >= 0)
                            return new Interval(Math.Pow(a.Lower, exponent), Math.Pow(a.Upper, exponent));
                        if (a.Upper < 0)
                            return new Interval(Math.Pow(a.Upper, exponent), Math.Pow(a.Lower, exponent));
                        return new Interval(
                            0,
                            Math.Max(Math.Pow(a.Upper, exponent), Math.Pow(a.Lower, exponent))
                            );
                    }
                    // odd exponent
                    return new Interval(Math.Pow(a.Lower, exponent), Math.Pow(a.Upper, exponent));
            }

        }

        public static Interval InvPower(Interval a, uint exponent)
        {
            if (exponent == 1)
                return a;

            var invExponent = 1.0 / exponent;

            if (exponent % 2 == 0)
            {
                // even exponent
                var lower = Math.Pow(Math.Max(0, a.Lower), invExponent);
                var upper = Math.Pow(Math.Max(0, a.Upper), invExponent);
                return new Interval(lower, upper);
            }
            // odd exponent
            return new Interval(NegativeTolerantPower(a.Lower, invExponent), NegativeTolerantPower(a.Upper, invExponent));

        }

        static double NegativeTolerantPower(double number, double exponent)
        {
            return Math.Sign(number) * Math.Pow(Math.Abs(number), exponent);
        }

        public Interval Square
        {
            get
            {
                var lowerSq = Lower * Lower;
                var upperSq = Upper * Upper;

                if (CrossesZero)
                    return new Interval(0, Math.Max(lowerSq, upperSq));

                if (Upper <= 0)
                    return new Interval(upperSq, lowerSq);

                return new Interval(lowerSq, upperSq);
            }
        }

        public static Interval PositiveSqrt(Interval a)
        {
            Debug.Assert(a.Lower >= 0, "Attempt to take square root of a negative interval");
            return new Interval(Math.Sqrt(a.Lower), Math.Sqrt(a.Upper));
        }

        private static double Min(double a, double b, double c, double d)
        {
            return Math.Min(Math.Min(a, b), Math.Min(c, d));
        }

        private static double Max(double a, double b, double c, double d)
        {
            return Math.Max(Math.Max(a, b), Math.Max(c, d));
        }

        private static double PropagatePositiveInfinity(double x, double otherwise)
        {
            return double.IsPositiveInfinity(x) ? double.PositiveInfinity : otherwise;
        }

/*
        private static double PropagatePositiveInfinity(double x, double y, double otherwise)
        {
            return (double.IsPositiveInfinity(x) || double.IsPositiveInfinity(y)) ? double.PositiveInfinity : otherwise;
        }
*/

        private static double PropagateNegativeInfinity(double x, double otherwise)
        {
            return double.IsNegativeInfinity(x) ? double.NegativeInfinity : otherwise;
        }

/*
        private static double PropagateNegativeInfinity(double x, double y, double otherwise)
        {
            return (double.IsNegativeInfinity(x) || double.IsNegativeInfinity(y)) ? double.NegativeInfinity : otherwise;
        }
*/

        public override bool Equals(object obj)
        {
            if (obj is Interval)
            {
                var i = (Interval)obj;
                // ReSharper disable CompareOfFloatsByEqualityOperator
                return this.Lower == i.Lower && this.Upper == i.Upper;
                // ReSharper restore CompareOfFloatsByEqualityOperator
            }
            return false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            return Lower.GetHashCode() ^ Upper.GetHashCode();
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(Interval a, Interval b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.Lower == b.Lower && a.Upper == b.Upper;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool operator !=(Interval a, Interval b)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return a.Lower != b.Lower || a.Upper != b.Upper;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public override string ToString()
        {
            if (Empty)
                return "Empty";
            if (IsUnique)
                return UniqueValue.ToString(CultureInfo.InvariantCulture);
            return string.Format("[{0}, {1}]", Lower, Upper);
        }

        public string DebugString
        {
            get
            {
                return this.ToString();
            }
        }

        public bool NearlyUnique
        {
            get
            {
                return !Empty && MathUtil.NearlyEqual(Lower, Upper, MathUtil.DefaultEpsilon);
            }
        }
    }
}

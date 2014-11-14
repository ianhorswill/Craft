#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Vector3Variable.cs" company="Ian Horswill">
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
    [DebuggerDisplay("{DebugString}")]
    public class Vector3Variable
    {
        public FloatVariable X;
        public FloatVariable Y;
        public FloatVariable Z;

        public readonly string Name;

        public Vector3Variable(string name, CSP p, BoundingBox b)
            : this(name, p, b.X, b.Y, b.Z)
        { }

        public Vector3Variable(string name, CSP p, double x, double y, double z)
            : this(name, p, new Interval(x), new Interval(y), new Interval(z)) 
        { }

        public Vector3Variable(string name, CSP p, Interval x, Interval y, Interval z)
        {
            Name = name;
            X = new FloatVariable(name + ".X", p, x);
            Y = new FloatVariable(name + ".Y", p, y);
            Z = new FloatVariable(name + ".Z", p, z);
        }

        public Vector3Variable(FloatVariable x, FloatVariable y, FloatVariable z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public CSP CSP
        {
            get
            {
                return X.CSP;
            }
        }

        public static Vector3Variable operator +(Vector3Variable a, Vector3Variable b)
        {
            return a.X.CSP.Memoize("+", () => new Vector3Variable(a.X + b.X, a.Y + b.Y, a.Z + b.Z), a, b);
        }

        public static Vector3Variable operator -(Vector3Variable a, Vector3Variable b)
        {
            return a.X.CSP.Memoize("-", () => new Vector3Variable(a.X - b.X, a.Y - b.Y, a.Z - b.Z), a, b);
        }

        public static Vector3Variable operator *(FloatVariable s, Vector3Variable v)
        {
            return s.CSP.Memoize("*", () =>  new Vector3Variable(s*v.X, s*v.Y, s*v.Z), s, v);
        }

        public static Vector3Variable operator *(Vector3Variable v, FloatVariable s)
        {
            return s.CSP.Memoize("*", () => new Vector3Variable(s * v.X, s * v.Y, s * v.Z), s, v);
        }

        public static Vector3Variable operator /(Vector3Variable v, FloatVariable s)
        {
            return s.CSP.Memoize("/", () => new Vector3Variable(v.X/s, v.Y/s, v.Z/s), s, v);
        }

        public static Vector3Variable operator *(double s, Vector3Variable v)
        {
            return v.X.CSP.Memoize("*", () =>  new Vector3Variable(s * v.X, s * v.Y, s * v.Z), s, v);
        }

        public static FloatVariable Dot(Vector3Variable a, Vector3Variable b)
        {
            return a.X.CSP.Memoize(
                "dot",
                () =>
                    {
                        var product = new FloatVariable(
                            string.Format("{0} dot {1}", a.Name, b.Name),
                            a.X.CSP,
                            a.X.Value * b.X.Value + a.Y.Value * b.Y.Value + a.Z.Value * b.Z.Value);
                        // ReSharper disable ObjectCreationAsStatement
                        new DotProductConstraint(product, a, b);
                        // ReSharper restore ObjectCreationAsStatement
                        return product;
                    },
                a,
                b);
        }

        public FloatVariable Magnitude
        {
            get
            {
                return X.CSP.Memoize(
                    "magnitude",
                    () =>
                        {
                            var magnitude = new FloatVariable(
                                "magnitude",
                                X.CSP,
                                Interval.PositiveSqrt(X.Value.Square + Y.Value.Square + Z.Value.Square));
// ReSharper disable ObjectCreationAsStatement
                            new MagnitudeConstraint(magnitude, this);
// ReSharper restore ObjectCreationAsStatement
                            return magnitude;
                        },
                    this);
            }
        }

        public void MustEqual(Vector3Variable v)
        {
            X.MustEqual(v.X);
            Y.MustEqual(v.Y);
            Z.MustEqual(v.Z);
        }

        public void MustBeParallel(Vector3Variable v)
        {
            var coefficient = new FloatVariable("parallelCoefficient", CSP, Interval.AllValues);
            this.MustEqual(coefficient * v);
        }

        public void MustBePerpendicular(Vector3Variable v)
        {
            Dot(this, v).MustEqual(0);
        }

        internal string DebugString
        {
            get
            {
                return string.Format(("{0} : ({1}, {2}, {3})"), Name, X.Value, Y.Value, Z.Value);
            }
        }

        /// <summary>
        /// Update component variables to point at their canonical variables
        /// </summary>
        public void CanonicalizeAndRegisterConstraint(Constraint c)
        {
            X = c.RegisterCanonical<FloatVariable, Interval>(X);
            Y = c.RegisterCanonical<FloatVariable, Interval>(Y);
            Z = c.RegisterCanonical<FloatVariable, Interval>(Z); 
        }
    }
}

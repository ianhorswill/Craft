#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MathUtil.cs" company="Ian Horswill">
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
    public static class MathUtil
    {
        public const double DefaultEpsilon = 0.000001f;

        public static bool NearlyEqual(double a, double b)
        {
            return NearlyEqual(a, b, DefaultEpsilon);
        }

        public static bool NearlyEqual(double a, double b, double epsilon)
        {
            // Cribbed from http://doubleing-point-gui.de/errors/comparison/

            double absA = Math.Abs(a);
            double absB = Math.Abs(b);
            double diff = Math.Abs(a - b);

// ReSharper disable CompareOfFloatsByEqualityOperator
            if (a == b)
                // shortcut, handles infinities
                return true;
            if (a == 0 || b == 0 || diff < double.Epsilon)
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < epsilon; // (epsilon * double.Epsilon);
            // use relative error
            return diff / (absA + absB) < epsilon;
// ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public static bool NearlyLE(double a, double b, double epsilon)
        {
            if (a <= b)  // Fastpath + < test
                return true;
            return NearlyEqual(a, b, epsilon);
        }

        public static bool NearlyGE(double a, double b, double epsilon)
        {
            if (a >= b)  // Fastpath + < test
                return true;
            return NearlyEqual(a, b, epsilon);
        }
    }
}

#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Randomizer.cs" company="Ian Horswill">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Craft;

using UnityEngine;

/// <summary>
/// Encapsulates a constraint problem from Craft as a Unity component
/// </summary>
[AddComponentMenu("Craft/Randomizer")]
public class Randomizer : MonoBehaviour
{
    #region Serialized by editor
    /// <summary>
    /// If true, this randomizer automatically solves for a set of random values
    /// and updates the attached components when the Randomier is loaded.  Otherwise
    /// it will wait until its Solve() method is called manually.
    /// </summary>
    public bool SolveOnLevelLoad = true;
    /// <summary>
    /// Variables to be randomized.
    /// </summary>
    public Variable[] Variables = { new Variable() };
    /// <summary>
    /// Constraints to apply to the variables.  These are stored as strings
    /// and parsed at level load time, since that's the easiest way to deal with
    /// serialization.
    /// </summary>
    public string[] Constraints = {""};
    /// <summary>
    /// The last values found for the variales.  This is only so they can be displayed in the editor.
    /// </summary>
    public string[] LastSolution;
    /// <summary>
    /// Number of millisecond taken to find the last solution.
    /// </summary>
    public float LastSolveTime;
    /// <summary>
    /// Maximum number of steps to try before giving up a restarting
    /// </summary>
    public int MaxSolverSteps = 1000;
    /// <summary>
    /// Maximum number of times to retry when solving.
    /// </summary>
    public int MaxRestarts = 20;
    #endregion

    #region Not serialized by editor
// ReSharper disable InconsistentNaming
    internal CSP CSP;
// ReSharper restore InconsistentNaming
    #endregion

    #region Accessors
    /// <summary>
    /// Returns the index of the variable with the specified variableName.
    /// </summary>
    /// <param name="variableName">the variableName of the desired variable</param>
    /// <param name="throwOnNotFound">If true, throw KeyNotFoundException if there is no variable with the specified name.</param>
    /// <returns>Its index in the variables array, or -1 if no such variable.</returns>
    public int VariableIndex(string variableName, bool throwOnNotFound = true)
    {
        for (int i = 0; i < Variables.Length; i++)
            if (Variables[i].VariableName == variableName)
                return i;
        throw new KeyNotFoundException("The Randomizer does not contain a variable named " + name);
    }
    /// <summary>
    /// Returns the value of a scalar variable given its position in the Variables array.
    /// </summary>
    /// <param name="variableIndex">Index within the Variables[].</param>
    /// <returns>Value of the variable.</returns>
    public double ScalarValue(int variableIndex)
    {
        return Variables[variableIndex].ScalarValue;
    }

    /// <summary>
    /// Returns the value of a scalar variable given its position in the Variables array.
    /// </summary>
    /// <param name="variableIndex">Index within the Variables[].</param>
    /// <returns>Value of the variable.</returns>
    public Vector3 Vector3Value(int variableIndex)
    {
        return Variables[variableIndex].Vector3Value;
    }

    /// <summary>
    /// Returns the value of a scalar variable given its variableName.
    /// </summary>
    /// <param name="variableName">Name in the variables array.</param>
    /// <returns>Value of the variable.</returns>
    public double ScalarValue(string variableName)
    {
        return Variables[this.VariableIndex(variableName)].ScalarValue;
    }

    /// <summary>
    /// Returns the value of a scalar variable given its variableName.
    /// </summary>
    /// <param name="variableName">Name in the variables array.</param>
    /// <returns>Value of the variable.</returns>
    public Vector3 Vector3Value(string variableName)
    {
        return Variables[this.VariableIndex(variableName)].Vector3Value;
    }
    #endregion

    /// <summary>
    /// Called at level load time.
    /// </summary>
    public void Start()
    {
        if (SolveOnLevelLoad)
            Solve();
    }


    private readonly Stopwatch timer = new Stopwatch();
    /// <summary>
    /// Finds a set of values for the variables.
    /// Call this to manually randomize if SolveOnLevelLoad is set to false.
    /// </summary>
    public void Solve()
    {
        if (CSP == null)
        {
            this.MakeCSP();
        }

        int retries = 0;
        bool done = false;

        timer.Reset();
        timer.Start();

        while (!done)
        {
            try
            {
                // ReSharper disable once PossibleNullReferenceException
                CSP.NewSolution();
                done = true;
            }
            catch (TimeoutException)
            {
                if (retries++ == MaxRestarts)
                    // Give up
                    throw;
            }
        }

        timer.Stop();
        LastSolveTime = (1000000f * timer.ElapsedTicks) / Stopwatch.Frequency;
        LastSolution = Variables.Select(v => v.ToString()).ToArray();
        WriteValuesToLinkedComponents();
    }

    /// <summary>
    /// Store variables in their designated component properties, if any
    /// </summary>
    private void WriteValuesToLinkedComponents()
    {
        foreach (var v in Variables)
            v.UpdateComponent();
    }

    /// <summary>
    /// Build the Craft.CSP object from the constraints and variables.
    /// </summary>
    void MakeCSP()
    {
        CSP = new CSP { MaxSteps = this.MaxSolverSteps };
        foreach (var v in Variables)
        {
            v.MakeVariable(CSP);
        }
        //this.DumpVariables();

        foreach (var c in Constraints)
        {
            if (c!=null && c.Trim() != "")
                BuildConstraint(c);
        }
        //this.DumpVariables();
    }

    #region Functions and operators
    private object ApplyParallel(List<object> args)
    {
        if (args.Count != 2) throw new ArgumentException("Wrong number of arguments to parallel(,).");
        var arg0 = args[0] as Vector3Variable;
        var arg1 = args[1] as Vector3Variable;
        if (arg0 == null || arg1 == null)
            throw new ArgumentException("Arguments to parallel(,) must be vectors");
        arg0.MustBeParallel(arg1);
        return "constraint";
    }

    private object ApplyPerpendicular(List<object> args)
    {
        if (args.Count != 2) throw new ArgumentException("Wrong number of arguments to perpendicular(,).");
        var arg0 = args[0] as Vector3Variable;
        var arg1 = args[1] as Vector3Variable;
        if (arg0 == null || arg1 == null)
            throw new ArgumentException("Arguments to perpendicular(,) must be vectors");
        arg0.MustBePerpendicular(arg1);
        return "constraint";
    }

    private object ApplyExponentiation(object lhs, object rhs)
    {
        if (!(lhs is FloatVariable))
            throw new ArgumentException("Lefthand argument of ^ must be a scalar variable.");
        if (!(rhs is double))
            throw new ArgumentException("Righthand argument of ^ must be a positive integer constant.");
        var d = (double)rhs;
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (d != (int)d || d < 1.0)
            // ReSharper restore CompareOfFloatsByEqualityOperator
            throw new ArgumentException("Righthand argument of ^ must be a positive integer constant.");
        return ((FloatVariable)lhs) ^ (uint)d;
    }

    private object ApplyDivision(object lhs, object rhs)
    {
        return ApplyMultiplicationLike(lhs, rhs,
            (a, b) => a / b,
            (a, b) => a / b,
            "Cannot divide {0} by {1} - incompatible types");
    }

    private object ApplyMultiplication(object lhs, object rhs)
    {
        if (lhs is FloatVariable && rhs is Vector3Variable)
            return ApplyMultiplication(rhs, lhs);
        return ApplyMultiplicationLike(lhs, rhs,
            (a, b) => a * b,
            (a, b) => a * b,
            "Cannot multiply {0} by {1} - incompatible types");
    }

    private object ApplyMultiplicationLike(object lhs, object rhs,
        Func<FloatVariable, FloatVariable, FloatVariable> scalarOp,
            Func<Vector3Variable, FloatVariable, Vector3Variable> vectorOp,
        string errorFormat)
    {
        lhs = Variablize(lhs);
        rhs = Variablize(rhs);

        if (BothScalar(lhs, rhs))
            return scalarOp((FloatVariable)lhs, (FloatVariable)rhs);

        if (VectorAndScalar(lhs, rhs))
            return vectorOp((Vector3Variable)lhs, (FloatVariable)rhs);

        throw new Exception(string.Format(errorFormat, lhs, rhs));
    }

    private object ApplyAddition(object lhs, object rhs)
    {
        return ApplyAdditionLike(lhs, rhs,
            (a, b) => a + b,
            (a, b) => a + b,
            "Cannot add {0} and {1} - incompatible types");
    }

    private object ApplySubtraction(object lhs, object rhs)
    {
        return ApplyAdditionLike(lhs, rhs,
            (a, b) => a - b,
            (a, b) => a - b,
            "Cannot subtract {1] from {0} - incompatible types");
    }

    private object ApplyAdditionLike(object lhs, object rhs, Func<FloatVariable, FloatVariable, FloatVariable> scalarOp, Func<Vector3Variable, Vector3Variable, Vector3Variable> vectorOp, string errorFormat)
    {
        lhs = Variablize(lhs);
        rhs = Variablize(rhs);

        if (BothScalar(lhs, rhs))
            return scalarOp((FloatVariable)lhs, (FloatVariable)rhs);

        if (BothVector(lhs, rhs))
            return vectorOp((Vector3Variable)lhs, (Vector3Variable)rhs);

        throw new ArgumentException(string.Format(errorFormat, lhs, rhs));
    }

    private object ApplyMagnitude(object o)
    {
        var vector = o as Vector3Variable;
        if (vector != null)
            return vector.Magnitude;
        throw new ArgumentException("|| operator can only be applied to vectors, but was used on " + o);
    }

    private object ApplyFunction(string functionName, List<object> args)
    {
        switch (functionName)
        {
            case "sqrt":
                return ApplySqrt(args);

            case "sum":
                return ApplySum(args);

            case "average":
            case "mean":
                return ApplyAverage(args);

            case "meanSquare":
                return ApplyMeanSquare(args);

            case "meanSquareDifference":
                return ApplyMeanSquareDifference(args);

            case "variance":
                return ApplyVariance(args);

            case "parallel":
                return this.ApplyParallel(args);

            case "perpendicular":
                return this.ApplyPerpendicular(args);

            case "horizontalProjection":
                return this.ApplyProjectHorizontal(args);

            default:
                throw new Exception("Unknown function: " + functionName);
        }
    }

    private object ApplySum(List<object> args)
    {
        if (args.Count == 1)
            return args[0];
        var result = this.ApplyAddition(args[0], args[1]);
        for (int i = 2; i < args.Count; i++)
            result = this.ApplyAddition(result, args[i]);
        return result;
    }

    private object ApplyAverage(List<object> args)
    {
        if (args.Count == 1)
            return args[0];
        var result = this.ApplyAddition(args[0], args[1]);
        for (int i = 2; i < args.Count; i++)
            result = this.ApplyAddition(result, args[i]);
        return (1.0 / args.Count) * ((FloatVariable)result);
    }

    private object ApplyMeanSquare(List<object> args)
    {
        if (args.Count == 1)
            return this.ApplyExponentiation(args[0], 2);
        var result = this.ApplyAddition(
            this.ApplyExponentiation(args[0], 2.0),
            this.ApplyExponentiation(args[1], 2.0)
            );
        for (int i = 2; i < args.Count; i++)
            result = this.ApplyAddition(result, this.ApplyExponentiation(args[i], 2.0));
        return (1.0 / args.Count) * ((FloatVariable)result);
    }

    private object ApplyMeanSquareDifference(List<object> args)
    {
        if (args.Count < 3) throw new ArgumentException("meanSquareDifference requires at least 3 arguments");
        var offset = this.Variablize(args[0]) as FloatVariable;
        if (offset == null)
            throw new ArgumentException("Arguments to meanSquareDifference must be scalars");
        var result = ((((FloatVariable)args[1]) - offset) ^ 2) + ((((FloatVariable)args[2]) - offset) ^ 2);
        for (int i = 3; i < args.Count; i++)
            result = result + ((((FloatVariable)args[i]) - offset) ^ 2);
        return (1.0 / (args.Count - 1)) * result;
    }

    private object ApplyVariance(List<object> args)
    {
        if (args.Count < 2) throw new ArgumentException("variance requires at least 2 arguments");
        var offset = this.ApplyAverage(args) as FloatVariable;
        var result = ((((FloatVariable)args[0]) - offset) ^ 2) + ((((FloatVariable)args[1]) - offset) ^ 2);
        for (int i = 2; i < args.Count; i++)
            result = result + ((((FloatVariable)args[i]) - offset) ^ 2);
        return (1.0 / args.Count) * result;
    }

    private object ApplyProjectHorizontal(List<object> args)
    {
        if (args.Count != 4) throw new ArgumentException("projectHorizontal requires 4 arguments: target, cameraLocation, opticAxis, focalLength");
        var target = args[0] as Vector3Variable;
        var cameraPosition = args[1] as Vector3Variable;
        var opticAxis = args[2] as Vector3Variable;
        var focalLength = args[3] as FloatVariable;
        if (target == null || cameraPosition == null || opticAxis == null || focalLength == null)
            throw new ArgumentException("Invalid argument type in call to Project(Vector3, Vector3, Vector3, float)");

        var offset = target - cameraPosition;
        var depth = Vector3Variable.Dot(opticAxis, offset);

        // Axis must be a unit vector
        opticAxis.Magnitude.MustEqual(1);
        // Target must be at least 1mm in front of the camera
        depth.MustBeContainedIn(new Interval(0.001, double.PositiveInfinity));

        var yProjection = focalLength*offset.Y / depth;
        var xProjection = focalLength*(offset.X*opticAxis.Z-offset.Z*opticAxis.X)/depth;
        return new Vector3Variable(xProjection, yProjection, FloatVariable.Constant(CSP, 1));
    }
    
    private object ApplySqrt(List<object> args)
    {
        if (args.Count != 1) throw new ArgumentException("Sqrt only takes one argument");
        var arg1 = args[0];
        var v = arg1 as FloatVariable;
        if (v == null)
            throw new ArgumentException("Argument to sqrt should be a scalar: " + arg1);
        throw new NotImplementedException();
    }
    #endregion

    #region Constraint tree building
    /// <summary>
    /// Parse the text of constraint and build the corresponding network of Constraint objects within the CSP.
    /// </summary>
    /// <param name="c">Text of the constraint</param>
    private void BuildConstraint(string c)
    {
        var tokenizer = new Tokenizer(c);
        var expression = this.ParseExpression(tokenizer);
        if (!tokenizer.EndOfTokens)
            throw new Exception("Extra token "+tokenizer.PeekToken()+" in constraint "+c);
        if (!expression.Equals("constraint"))
            throw new Exception("Expression is not a constraint: " + c);
    }

    /// <summary>
    /// Creates the constraint object associated with operation OP on the specified arguments
    /// and returns the FloatVariable or Vector3Variable representing its result.
    /// </summary>
    /// <param name="op"></param>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    private object ApplyOperator(string op, object lhs, object rhs)
    {
        switch (op)
        {
            case "=":
                return ApplyEquals(lhs, rhs);

            case "<=":
                return ApplyLessThanEqual(lhs, rhs);

            case ">=":
                return ApplyLessThanEqual(rhs, lhs);

            case "+":
                return ApplyAddition(lhs, rhs);

            case "-":
                return ApplySubtraction(lhs, rhs);

            case "*":
                return ApplyMultiplication(lhs, rhs);

            case "/":
                return ApplyDivision(lhs, rhs);

            case "^":
                return ApplyExponentiation(lhs, rhs);

            default:
                throw new NotImplementedException("Unsupported operator: "+op);
        }
    }

    private static object ApplyEquals(object lhs, object rhs)
    {
        if (IsVariable(lhs) && rhs is double)
        {
            EquateVariableConstant(lhs, (double)rhs);
        }
        else if (lhs is double && IsVariable(rhs))
            EquateVariableConstant(rhs, (double)lhs);
        else
        {
            EquateVariables(lhs, rhs);
        }
        return "constraint";
    }

    private static object ApplyLessThanEqual(object lhs, object rhs)
    {
        var lvar = lhs as FloatVariable;
        var rvar = rhs as FloatVariable;
        if (lvar != null && rvar == null)
        {
            // var < constant
            if (!(rhs is double))
                throw new ArgumentException("Invalid type in inequality; should be a number: "+rhs);
            lvar.MustBeContainedIn(new Interval(double.NegativeInfinity, (double)rhs));
        }
        else if (lvar == null && rvar != null)
        {
            // constant < var
            if (!(lhs is double))
                throw new ArgumentException("Invalid type in inequality; should be a number: " + lhs);
            rvar.MustBeContainedIn(new Interval((double)lhs, double.PositiveInfinity));
        }
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        else if (lvar != null && rvar != null)
        {
            // var < var
            (rvar - lvar).MustBeContainedIn(new Interval(0, float.PositiveInfinity));
        }
        else
        {
            throw new ArgumentException("Unsupported arguments to <= or >=: "+lhs+rhs);
        }
        return "constraint";
    }

    private static void EquateVariableConstant(object o, double c)
    {
        var v = o as FloatVariable;
        if (v != null)
            v.MustEqual(c);
        else throw new Exception("Cannot equate a vector with a number: " + o);
    }

    private static void EquateVariables(object v1, object v2)
    {
        if (BothScalar(v1, v2))
            ((FloatVariable)v1).MustEqual((FloatVariable)v2);
        else if (BothVector(v1, v2))
            ((Vector3Variable)v1).MustEqual((Vector3Variable)v2);
        else throw new Exception("Cannot equate valautes of differing types: " + v1 + ", " + v2);
    }
    #endregion

    #region Constraint parser
    /// <summary>
    /// Top-level entry point for parsing.
    /// This is an operator-precedence parser based on a simplified version
    /// of the algorithm in wikipedia.
    /// </summary>
    /// <param name="tokens">Token stream</param>
    /// <returns>Result of parsed/executed expression</returns>
    object ParseExpression(Tokenizer tokens)
    {
        return this.ParseOperatorExpression(ParsePrimary(tokens), tokens, 0);
    }

    /// <summary>
    /// Handles infix operators.  Should not be called from anyplace but ParseExpression.
    /// Does not currently handle unary or right-associative operators.
    /// </summary>
    /// <param name="lhs">Value of the left-hand side argument to the operator.</param>
    /// <param name="tokens">Token stream</param>
    /// <param name="minPrecedence">The precedence of any infix operator of which this is the rhs.</param>
    /// <returns>Value of the parsed expression</returns>
    private object ParseOperatorExpression(object lhs, Tokenizer tokens, int minPrecedence)
    {
        while (!tokens.EndOfTokens && IsBinaryOperator(tokens.PeekToken()) && Precedence(tokens.PeekToken()) >= minPrecedence)
        {
            string op = tokens.NextToken();
            object rhs = this.ParsePrimary(tokens);
            while (!tokens.EndOfTokens && IsBinaryOperator(tokens.PeekToken())
                   && Precedence(tokens.PeekToken()) > Precedence(op))
            {
                rhs = this.ParseOperatorExpression(rhs, tokens, Precedence(tokens.PeekToken()));
            }
            lhs = ApplyOperator(op, lhs, rhs);
        }
        return lhs;
    }

    private object ParsePrimary(Tokenizer tokens)
    {
        var nextToken = tokens.NextToken();
        switch (nextToken)
        {
            case "-":
                throw new Exception("Unary 1 not yet implemented");

            case "(":
                {
                    var result = this.ParseExpression(tokens);
                    if (tokens.PeekTokenOrEOT() != ")")
                        throw new Exception("Parse error - expected ')', but got "+tokens.PeekTokenOrEOT());
                    tokens.NextToken();
                    return result;
                }

            case "|":
                {
                    var result = this.ParseExpression(tokens);
                    if (tokens.PeekTokenOrEOT() != "|")
                        throw new Exception("Parse error - expected '|' bot got "+tokens.PeekTokenOrEOT());
                    tokens.NextToken();
                    return ApplyMagnitude(result);
                }

            default:
                if (tokens.PeekTokenOrEOT() == "(")
                    return ParseFunctionCall(nextToken, tokens);
                return this.ResolveAtomic(nextToken);
        }
    }

    /// <summary>
    /// Parses prefix formal function calls, e.g. f(arg).
    /// Called after function name read but before ( is read.
    /// </summary>
    /// <param name="functionName">Name of function being called</param>
    /// <param name="tokens">Tokenizer</param>
    /// <returns>Variable representing the result of the function.</returns>
    private object ParseFunctionCall(string functionName, Tokenizer tokens)
    {
        tokens.NextToken();  // Skip open paren
        var args = new List<object> { this.ParseExpression(tokens) };
        while (tokens.PeekToken() == ",")
        {
            tokens.NextToken();   // Skip comma
            args.Add(this.ParseExpression(tokens));
        }
        if (tokens.PeekToken() == ")")
        {
            tokens.NextToken();   // Skip close paren
            return this.ApplyFunction(functionName, args);
        }
        throw new Exception("Expected ) but got "+tokens.PeekToken());
    }

    /// <summary>
    /// Parses a constant or variable name
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The variable or constant (a double)</returns>
    object ResolveAtomic(string value)
    {
        double asNumber;
        if (double.TryParse(value, out asNumber))
            return this.ResolveConstant(value);
        return this.ResolveVariableReference(value);
    }

    object ResolveVariableReference(string variableName)
    {
        foreach (var v in Variables)
            if (v.VariableName == variableName)
                return v.FloatVariableOrVector;
        throw new Exception("Unknown variable: "+variableName);
    }

    double ResolveConstant(string value)
    {
        return double.Parse(value);
    }

    /// <summary>
    /// True if op is a binary operator
    /// </summary>
    static bool IsBinaryOperator(string op)
    {
        switch (op)
        {
            case "=":
            case "<=":
            case ">=":
            case "+":
            case "-":
            case "*":
            case "/":
            case "^":
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Returns the precedence of op
    /// </summary>
    static int Precedence(string op)
    {
        switch (op)
        {
            case "=":
            case "<=":
            case ">=":
                return 0;

            case "+":
            case "-":
                return 1;

            case "*":
            case "/":
                return 2;

            case "^":
                return 3;

            default:
                throw new Exception("Unknown operator: " + op);
        }
    }

    /// <summary>
    /// A very quick and dirty tokenizer for constraints.
    /// This could definitely be optimized, but it's probably fast compared to the rest what Unity
    /// is doing to load a level.
    /// </summary>
    class Tokenizer
    {
        /// <summary>
        /// Array of the tokens extracted from the string.
        /// We carve the string up in advance into tokens using a RegEx rather than manually
        /// build a state machine.  I'm lazy.
        /// </summary>
        private readonly string[] tokens;
        /// <summary>
        /// Index within tokens[] for the next token to be read.
        /// </summary>
        private int position;
        /// <summary>
        /// Text of the constraint.  Used only for generating error messages.
        /// </summary>
        private readonly string originalText;

        /// <summary>
        /// The RegEx used to define tokens
        /// </summary>
        private static readonly Regex TokenPattern = new Regex(@"(<=|>=|[\+\-\*\/\^\(\),=])");

        /// <summary>
        /// Makes a tokenizer from the string
        /// </summary>
        /// <param name="text">The string to parse into tokens.</param>
        public Tokenizer(string text)
        {
            originalText = text;
            //tokens = text.Split(new[] { ' ' });
            tokens = TokenPattern.Split(text).Select(s => s.Trim()).Where(s => s != "").ToArray();
            //UnityEngine.Debug.Log(ShowTokens(tokens));
        }

        //string ShowTokens(string[] a)
        //{
        //    var s = "";
        //    foreach (var t in a)
        //        s = s + "|" + t;
        //    return s + "|";

        //}

        /// <summary>
        /// True if we've read all the tokens.
        /// </summary>
        public bool EndOfTokens
        {
            get
            {
                return position == tokens.Length;
            }
        }

        /// <summary>
        /// Get the next token
        /// </summary>
        /// <returns>The token (a string)</returns>
        public string NextToken()
        {
            if (!EndOfTokens)
                return tokens[position++];
            throw new ArgumentException(string.Format("Constraint text ends prematurely: \"{0}\"", originalText));
        }

        /// <summary>
        /// Returns the next token without advancing to the following token.
        /// </summary>
        /// <returns></returns>
        public string PeekToken()
        {
            return tokens[position];
        }

        /// <summary>
        /// Returns the next token or "end of expression" if all tokens have been read.
        /// </summary>
        /// <returns>The token</returns>
        public string PeekTokenOrEOT()
        {
            if (EndOfTokens)
                return "end of expression";
            return this.PeekToken();
        }
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Converts float constants to FloatVariables
    /// </summary>
    /// <param name="x">Value (variable or constant)</param>
    /// <returns>Original value or FloatVariable if the original value had been a constant.</returns>
    object Variablize(object x)
    {
        if (x is double)
        {
            return FloatVariable.Constant(CSP, (double)x);
        }
        return x;
    }

    /// <summary>
    /// True if both arguments are FloatVariables
    /// </summary>
    static bool BothScalar(object o1, object o2)
    {
        return o1 is FloatVariable && o2 is FloatVariable;
    }

    /// <summary>
    /// True if both arguments are Vector3Variables
    /// </summary>
    static bool BothVector(object o1, object o2)
    {
        return o1 is Vector3Variable && o2 is Vector3Variable;
    }

    /// <summary>
    /// True if the first argument is a Vector3Variable and the second a FloatVariable
    /// </summary>
    static bool VectorAndScalar(object o1, object o2)
    {
        return o1 is Vector3Variable && o2 is FloatVariable;
    }

    /// <summary>
    /// True if  argument is a FloatVariable
    /// </summary>
    static bool IsVariable(object o)
    {
        return o is FloatVariable || o is Vector3Variable;
    }

    //private void DumpVariables()
    //{
    //    foreach (var v in this.Variables)
    //    {
    //        Debug.Log(v.VariableName + "=" + v.FloatVariable.Value);
    //    }
    //}
    #endregion
}

/// <summary>
/// Encapsulates a Craft FloatVariable or Vector3Variable,
/// optionally with information about what Unity component's property
/// to store it to
/// </summary>
[Serializable]
public class Variable
{
    #region Serialized by the editor
    public string VariableName;
    /// <summary>
    /// The component containing the field
    /// </summary>
    public Component Component;
    /// <summary>
    /// The name of the field or property
    /// </summary>
    public string PropertyName;

    // Horrible kluge because Unity can't serialize variant types or even structs.
    public float Min;

    public float Max;

    public float MinY;

    public float MaxY;

    public float MinZ;

    public float MaxZ;
    #endregion

    #region Unserialized by the editor
    [NonSerialized]
    private MemberInfo memberInfoCache;
    [NonSerialized]
    FloatVariable mFloatVariable;
    [NonSerialized]
    Vector3Variable mVector3Variable;
    #endregion

    #region Public properties
    /// <summary>
    /// The value of this variable chosen by the solver,
    /// assuming it is a scalar (non-vector) variable
    /// </summary>
    public double ScalarValue
    {
        get
        {
            return FloatVariable.UniqueValue;
        }
    }

    /// <summary>
    /// The value of this variable chosen by the solver,
    /// assuming it is a vector variable
    /// </summary>
    public Vector3 Vector3Value
    {
        get
        {
            var v = Vector3Variable;
            return new Vector3((float)v.X.UniqueValue, (float)v.Y.UniqueValue, (float)v.Z.UniqueValue);
        }
    }
    #endregion

    #region Reflection interface - writes data back to another unity component
    /// <summary>
    /// The MemberInfo object for the specified field or property.
    /// Throws MissingMemberException if there is no such field.
    /// </summary>
    MemberInfo MemberInfo
    {
        get
        {
            if (string.IsNullOrEmpty(PropertyName))
                PropertyName = VariableName;
            if (memberInfoCache == null
                || memberInfoCache.Name != PropertyName
// ReSharper disable AssignNullToNotNullAttribute
                || !Component.GetType().IsSubclassOf(memberInfoCache.DeclaringType))
// ReSharper restore AssignNullToNotNullAttribute
            {
                memberInfoCache = this.Component.GetType().GetProperty(this.PropertyName)
                                  ?? (MemberInfo)this.Component.GetType().GetField(this.PropertyName);
            }
            if (memberInfoCache == null)
                throw new MissingMemberException(Component.GetType().Name, PropertyName);
            return memberInfoCache;
        }
    }

    /// <summary>
    /// The FieldInfo for PropertyName, or null if it isn't a field.
    /// </summary>
    FieldInfo FieldInfo
    {
        get
        {
            return MemberInfo as FieldInfo;
        }
    }

    /// <summary>
    /// The PropertyInfo for PropertyName, or null if it isn't a property.
    /// </summary>
    PropertyInfo PropertyInfo
    {
        get
        {
            return MemberInfo as PropertyInfo;
        }
    }

    /// <summary>
    /// The type of the referenced field or property
    /// </summary>
    public Type Type
    {
        get
        {
            var f = FieldInfo;
            if (f != null)
                return f.FieldType;
            return PropertyInfo.PropertyType;
        }
    }

    internal void UpdateComponent()
    {
        if (Component != null)
        {
            if (mFloatVariable != null)
                this.SetValue(mFloatVariable.UniqueValue);
            else if (mVector3Variable != null)
                this.SetValue(new Vector3((float)mVector3Variable.X.UniqueValue, (float)mVector3Variable.Y.UniqueValue, (float)mVector3Variable.Z.UniqueValue));
        }

    }
    #endregion

    /// <summary>
    /// Returns the internal Craft.Variable corresponding to this Variable
    /// </summary>
    public object FloatVariableOrVector
    {
        get
        {
            return (object)mFloatVariable ?? mVector3Variable;
        }
    }
    
    /// <summary>
    /// Returns the internal Craft.FloatVariable corresponding to this Variable
    /// </summary>
    public FloatVariable FloatVariable
    {
        get
        {
            if (this.mFloatVariable != null)
                return this.mFloatVariable;
            throw new InvalidOperationException("Variable "+VariableName+" is not a float");
        }
    }

    /// <summary>
    /// Returns the internal Craft.Vector3Variable corresponding to this Variable
    /// </summary>
    public Vector3Variable Vector3Variable
    {
        get
        {
            if (this.mVector3Variable != null)
                return this.mVector3Variable;
            throw new InvalidOperationException("Variable "+VariableName+" is not a Vector3");
        }
    }

    /// <summary>
    /// Update the value of the specified field or property of the specified object.
    /// </summary>
    /// <param name="newValue">New value to assign to the field or property</param>
    public void SetValue(object newValue)
    {
        var tName = Type.Name;
        var f = FieldInfo;

        if (f != null)
        {
            switch (tName)
            {
                case "Single":
                    f.SetValue(Component, Convert.ToSingle(newValue));
                    break;

                case "Double":
                    f.SetValue(Component, Convert.ToDouble(newValue));
                    break;

                case "Vector3":
                    if (newValue is Vector3)
                        f.SetValue(Component, newValue);
                    else
                        throw new InvalidOperationException(string.Format("Property {0} of component {1} is a Vector but {2} is not.",
                                                            PropertyName, Component.GetType().Name, VariableName));
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Property {0} of component {1} is of type {2} but Randomizer variables must be numbers or vectors.",
                                                        PropertyName, Component.GetType().Name, tName));
            }
        }
        else
        {
            var p = PropertyInfo;
            switch (tName)
            {
                case "Single":
                    p.SetValue(Component, Convert.ToSingle(newValue), null);
                    break;

                case "Double":
                    p.SetValue(Component, Convert.ToDouble(newValue), null);
                    break;

                case "Vector3":
                    if (newValue is Vector3)
                        p.SetValue(Component, newValue, null);
                    else
                        throw new InvalidOperationException(string.Format("Property {0} of component {1} is a Vector but {2} is not.",
                                                            PropertyName, Component.GetType().Name, VariableName));
                    break;

                default:
                    throw new InvalidOperationException(string.Format("Property {0} of component {1} is of type {2} but Randomizer variables must be numbers or vectors.",
                                                        PropertyName, Component.GetType().Name, tName));
            }
        }
    }

    internal void MakeVariable(CSP csp)
    {
        if (Component != null)
        {
            if (Type == typeof(float))
                MakeFloatVariable(csp);
            else if (Type == typeof(Vector3))
                MakeVector3Variable(csp);
            else
                throw new Exception(string.Format("Variable {0} must be a float or Vector3 variable", VariableName));
        }
        else
        {
// ReSharper disable CompareOfFloatsByEqualityOperator
            if (MinY == 0 && MinZ == 0 && MaxY == 0 && MaxZ == 0)
// ReSharper restore CompareOfFloatsByEqualityOperator
                this.MakeFloatVariable(csp);
            else
                this.MakeVector3Variable(csp);
        }
    }

    private void MakeVector3Variable(CSP csp)
    {
        this.mVector3Variable = new Vector3Variable(VariableName, csp, new BoundingBox(new Interval(Min, Max), new Interval(MinY, MaxY), new Interval(MinZ, MaxZ)));
    }

    private void MakeFloatVariable(CSP csp)
    {
        this.mFloatVariable = new FloatVariable(VariableName, csp, new Interval(Min, Max));
    }

    public override string ToString()
    {
        if (mFloatVariable != null)
            return string.Format("{0}={1}", VariableName, mFloatVariable.Value);
        if (mVector3Variable != null)
            return string.Format("{0}=({1}, {2}, {3})", VariableName, mVector3Variable.X.Value, mVector3Variable.Y.Value, mVector3Variable.Z.Value);
        return VariableName;
    }
}
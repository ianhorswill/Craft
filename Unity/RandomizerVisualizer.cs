#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomizerVisualizer.cs" company="Ian Horswill">
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

using UnityEngine;

using Debug = System.Diagnostics.Debug;

/// <summary>
/// Creates scatter plots of Randomizer output using a particle system
/// </summary>
[AddComponentMenu("Craft/Randomizer visualizer")]
public class RandomizerVisualizer : MonoBehaviour
{
    public Randomizer Randomizer;
    public string XVariable, YVariable, ZVariable;
    public int EmitRate = 1;

    public float Scale = 1;

    private int xVariableIndex, yVariableIndex, zVariableIndex;

    internal void Start()
    {
        if (Randomizer == null)
            Randomizer = this.GetComponent<Randomizer>();
        if (particleSystem == null)
        {
            gameObject.AddComponent<ParticleSystem>();
            Debug.Assert(particleSystem != null, "particleSystem != null");
            particleSystem.enableEmission = false;
        }
        xVariableIndex = 0;
        yVariableIndex = 1;
        zVariableIndex = -1;
        solveCount = 0;
        minSolveTime = maxSolveTime = this.totalSolveTime = 0;
    }

    // Execution time stats
    private float minSolveTime;
    private float maxSolveTime;
    private float totalSolveTime;
    private int solveCount;
    
    /// <summary>
    /// Compute some new solutions and emit them as particles.
    /// </summary>
    internal void Update()
    {
        for (int i = 0; i < EmitRate; i++)
        {
#if SuppressTimingAfterGC
            var gcCount = GC.CollectionCount(0);
#endif
            this.Randomizer.Solve();
            var solveTime = this.Randomizer.LastSolveTime;
            // Update stats unless there happened to be a GC during the solver run.
#if SuppressTimingAfterGC
            if (GC.CollectionCount(0) == gcCount)
            {
#endif
                if (solveCount == 0)
                {
                    minSolveTime = maxSolveTime = totalSolveTime = solveTime;
                }
                else
                {
                    totalSolveTime += solveTime;
                    if (solveTime > maxSolveTime)
                        maxSolveTime = solveTime;
                    if (solveTime < minSolveTime)
                        minSolveTime = solveTime;
                }
                solveCount++;
#if SuppressTimingAfterGC
            }
#endif
            var position = new Vector3(
                (float)this.Randomizer.Variables[xVariableIndex].FloatVariable.UniqueValue,
                (float)this.Randomizer.Variables[yVariableIndex].FloatVariable.UniqueValue,
                zVariableIndex < 0 ? 0 : (float)this.Randomizer.Variables[zVariableIndex].FloatVariable.UniqueValue);
            particleSystem.Emit(Scale*position, Vector3.zero, 0.1f, 1, Color.yellow);
        }
    }

    internal void OnGUI()
    {
        GUILayout.Label(string.Format("Average solve time: {0}usec\nMin: {1}\nMax: {2}\nTotal variables: {3}\nTotal constraints: {4}",
            totalSolveTime/solveCount,
            minSolveTime,
            maxSolveTime,
            this.Randomizer.CSP.VariableCount,
            this.Randomizer.CSP.ConstraintCount));
    }
}

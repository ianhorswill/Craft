#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomizerTester.cs" company="Ian Horswill">
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
using System.IO;
using System.Linq;

using UnityEngine;

[AddComponentMenu("Craft/Randomizer tester")]
public class RandomizerTester : MonoBehaviour
{
    public Randomizer Randomizer;

    public int NumberOfSolutions;

    // ReSharper disable once InconsistentNaming
    public string CSVFileName;

    public void Start()
    {
        if (Randomizer == null)
            Randomizer = this.GetComponent<Randomizer>();
        if (Path.GetExtension(CSVFileName) == "")
            CSVFileName = CSVFileName + ".csv";
        using (var file = File.CreateText(CSVFileName))
        {
            WriteCSVLine(file, Randomizer.Variables.Select(v => v.VariableName));
            for (int i = 0; i < NumberOfSolutions; i++)
            {
                Randomizer.Solve();
                WriteCSVLine(file, Randomizer.Variables.Select(v => v.FloatVariable.UniqueValue));
            }
        }
    }

    void WriteCSVLine(TextWriter file, IEnumerable items)
    {
        bool first=true;
        foreach (var item in items)
        {
            if (!first)
                file.Write(',');
            else
                first = false;
            file.Write('"');
            file.Write(item);
            file.Write('"');
        }
        file.Write("\r\n");
    }
}

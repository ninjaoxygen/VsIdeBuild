#region License

/*
 * File: Program.cs
 *
 * The MIT License
 *
 * Copyright © 2017 - 2019 AVSP Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

#endregion License

using System;
using System.Diagnostics;
using AVSP.ConsoleSupport;
using VsIdeBuild.VsBuilderLibrary;

namespace VsIdeBuild
{
    internal class Program
    {
        private static int Run(SimpleArguments arguments)
        {
            VsBuilderOptions options = new VsBuilderOptions();

            // parse command line arguments into options
            SimpleArgumentsReader.ArgumentsToObject(arguments, options);

            if (options.Solution == null || options.Solution.Length == 0)
            {
                Console.Error.WriteLine(@"ERROR: solution must be specified with -solution c:\path\to\solution.sln");
                return 1;
            }

            VsBuilder builder = new VsBuilder();

            int result = builder.Run(options);

            if ((result != 0) || builder.Results.Failed)
            {
                Console.Error.WriteLine("ERROR: some builds failed, check output");
                return 1;
            }

            Console.WriteLine("Success: all okay");
            return 0;
        }

        // STAThread needed for COM
        [STAThread]
        private static int Main(string[] args)
        {
            return ConsoleHelper.RunProgram(args, Run);
        }
    }
}
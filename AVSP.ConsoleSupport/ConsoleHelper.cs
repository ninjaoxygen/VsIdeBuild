#region License

/*
 * File: ConsoleHelper.cs
 *
 * The MIT License
 *
 * Copyright © 2017 AVSP Ltd
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AVSP.ConsoleSupport
{
    /// <summary>
    /// Console support library for banner, copyright, version, command line arguments
    /// </summary>
    internal static class ConsoleHelper
    {
        public delegate int RunFunction(SimpleArguments arguments);

        public static SimpleArguments Arguments { get; private set; }

        /// <summary>
        /// Runs a console app with parsed options, sensible exit codes, exception handler
        ///
        /// Example:
        /// private static int Main(string[] args)
        /// {
        ///   return ConsoleHelper.RunProgram(args, Run);
        /// }
        /// </summary>
        /// <param name="args">arguments from Main()</param>
        /// <param name="run">delegate to run app</param>
        /// <returns>program exit code</returns>
        public static int RunProgram(IList<string> args, RunFunction run)
        {
            try
            {
                ConsoleHelper.Startup(args);
                ConsoleHelper.WriteBanner();

                int returnValue = run(ConsoleHelper.Arguments);

                return (Environment.ExitCode == 0) ? returnValue : Environment.ExitCode;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Trace.TraceError(e.ToString());

                return Environment.ExitCode != 0
                     ? Environment.ExitCode : 100;
            }
        }

        public static void Startup(IList<string> args)
        {
            Arguments = new SimpleArguments(args);

            // enable trace with v parameter
            if (Arguments.GetFlag("v"))
            {
                Trace.Listeners.Add(new ConsoleTraceListener(true));
            }
        }

        public static string GetProductVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static string GetProductName()
        {
            return Assembly.GetExecutingAssembly().GetName().Name;
        }

        public static string GetDescription()
        {
            //Type of attribute that is desired
            Type type = typeof(AssemblyDescriptionAttribute);

            //Is there an attribute of this type already defined?
            if (AssemblyDescriptionAttribute.IsDefined(Assembly.GetExecutingAssembly(), type))
            {
                //if there is, get attribute of desired type
                AssemblyDescriptionAttribute assemblyDescriptionAttribute = (AssemblyDescriptionAttribute)AssemblyDescriptionAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), type);

                return assemblyDescriptionAttribute.Description;
            }

            return null;
        }

        public static string GetCopyright()
        {
            //Type of attribute that is desired
            Type type = typeof(AssemblyCopyrightAttribute);

            //Is there an attribute of this type already defined?
            if (AssemblyCopyrightAttribute.IsDefined(Assembly.GetExecutingAssembly(), type))
            {
                //if there is, get attribute of desired type
                AssemblyCopyrightAttribute assemblyCopyrightAttribute = (AssemblyCopyrightAttribute)AssemblyCopyrightAttribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), type);

                return assemblyCopyrightAttribute.Copyright;
            }

            return null;
        }

        public static void WriteBanner()
        {
            Console.WriteLine(GetProductName() + " v" + GetProductVersion());
            Console.WriteLine(GetCopyright());
            Console.WriteLine();
        }
    }
}
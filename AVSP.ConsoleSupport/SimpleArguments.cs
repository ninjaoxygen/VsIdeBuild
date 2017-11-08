#region License

/*
 * File: SimpleArguments.cs
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
using System.Collections.ObjectModel;
using System.Linq;

namespace AVSP.ConsoleSupport
{
    /// <summary>
    /// Handles sets of arguments specified with a - and an optional parameter for each argument when no dash given
    /// </summary>
    public class SimpleArguments
    {
        protected Dictionary<string, string> argumentValues = new Dictionary<string, string>();
        protected Dictionary<string, bool> flags = new Dictionary<string, bool>();

        public ReadOnlyCollection<string> Arguments { get; protected set; }

        public SimpleArguments()
        {
            Arguments = new List<string>().AsReadOnly();
        }

        /// <summary>
        /// Constructor which parses command line arguments
        /// </summary>
        /// <param name="args">Args array from Program.Main()</param>
        public SimpleArguments(IList<string> args)
        {
            Parse(args);
        }

        /// <summary>
        /// Set default value for an argument, will not create a flag
        /// </summary>
        /// <param name="argumentName">Name of argument to set value for</param>
        /// <param name="value">Value to set argument to</param>
        public void SetDefault(string argumentName, string value)
        {
            if (!argumentValues.ContainsKey(argumentName))
            {
                argumentValues[argumentName] = value;
            }
        }

        /// <summary>
        /// Parse a list of argument values, e.g. { "-argumentname", "argumentvalue", "-flagtoset" }
        /// </summary>
        /// <param name="args">List of arguments to parse</param>
        public void Parse(IList<string> args)
        {
            // list of non-option arguments
            List<string> arguments = new List<string>();

            string lastArgumentName = null;

            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    lastArgumentName = arg.Substring(1).ToLower();
                    flags[lastArgumentName] = true;
                }
                else
                {
                    if (lastArgumentName != null && lastArgumentName.Length > 0)
                    {
                        argumentValues[lastArgumentName] = arg;
                        lastArgumentName = null;
                    }
                    else
                    {
                        arguments.Add(arg);
                    }
                }
            }

            Arguments = arguments.AsReadOnly();
        }

        /// <summary>
        /// Get the value of a passed argument, or the default
        /// </summary>
        /// <param name="argumentName">Name of argument to set value for</param>
        /// <returns>string value of argument</returns>
        public string GetValue(string argumentName)
        {
            string value;

            argumentValues.TryGetValue(argumentName.ToLower(), out value);

            return value;
        }

        /// <summary>
        /// Get whether a flag was passed
        /// </summary>
        /// <param name="argumentName">Name of argument to check for</param>
        /// <returns>true when argument was passed</returns>
        public bool GetFlag(string argumentName)
        {
            return flags.ContainsKey(argumentName.ToLower());
        }

        /// <summary>
        /// Get list of values in a comma-separated option
        /// </summary>
        /// <param name="argumentName">argument to retrieve</param>
        /// <returns>list of string contained in argument</returns>
        public IEnumerable<string> GetList(string argumentName)
        {
            string value;
            char[] charSplit = new char[] { ',' };

            if (argumentValues.TryGetValue(argumentName.ToLower(), out value))
            {
                return value.Split(charSplit, StringSplitOptions.RemoveEmptyEntries);
            }

            return Enumerable.Empty<string>();
        }
    }
}
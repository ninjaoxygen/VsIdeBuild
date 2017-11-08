#region License

/*
 * File: SimpleArgumentsReader.cs
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
    /// Reads arguments into POCOs, supports fields or properties of int, string, int[], string[], other type may work too
    /// </summary>
    public static class SimpleArgumentsReader
    {
        private static Type boolType = typeof(bool);

        /// <summary>
        /// Read a set of command line arguments into the properties or fields in a POCO
        /// </summary>
        /// <param name="args">source list of command line arguments, like the array from Main()</param>
        /// <param name="value">destination POCO to set values in</param>
        public static void ArgumentsToObject(IList<string> args, object value)
        {
            SimpleArguments arguments = new SimpleArguments(args);
            ArgumentsToObject(arguments, value);
        }

        /// <summary>
        /// Read set of parsed SimpleArguments into the properties or fields in a POCO
        /// </summary>
        /// <param name="arguments">SimpleArguments objects with source values</param>
        /// <param name="value"destination POCO to set values in></param>
        public static void ArgumentsToObject(SimpleArguments arguments, object value)
        {
            char[] commaSplit = new char[] { ',' };

            Type t = value.GetType();

            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                MemberInfo m = field;

                try
                {
                    if (field.FieldType == boolType)
                    {
                        field.SetValue(value, arguments.GetFlag(field.Name));
                    }
                    else
                    {
                        string valueString = arguments.GetValue(field.Name);

                        if (valueString != null)
                        {
                            field.SetValue(value, Convert.ChangeType(valueString, field.FieldType));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("ERROR: exception occurred whilst parsing argument " + field.Name, ex);
                }
            }

            PropertyInfo[] properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    if (property.PropertyType == boolType)
                    {
                        // handle a normal bool, if the flag was passed, set it to true, if not, false
                        property.SetValue(value, arguments.GetFlag(property.Name), null);
                    }
                    else if (property.PropertyType.BaseType.FullName == "System.Array")
                    {
                        // handle comma separated arrays
                        string valueString = arguments.GetValue(property.Name);

                        if (valueString != null)
                        {
                            string[] valueStringArray = valueString.Split(commaSplit);

                            int arrayLength = valueStringArray.Length;

                            Type elementType = property.PropertyType.GetElementType();

                            // build the array
                            Array y = Array.CreateInstance(elementType, arrayLength);

                            for (int i = 0; i < arrayLength; i++)
                            {
                                // set an element
                                y.SetValue(Convert.ChangeType(valueStringArray[i], elementType), i);
                            }

                            // put the array into the property
                            property.SetValue(value, y, null);
                        }
                    }
                    else
                    {
                        string valueString = arguments.GetValue(property.Name);

                        if (valueString != null)
                        {
                            property.SetValue(value, Convert.ChangeType(valueString, property.PropertyType), null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("ERROR: exception occurred whilst parsing argument " + property.Name, ex);
                }
            }
        }
    }
}
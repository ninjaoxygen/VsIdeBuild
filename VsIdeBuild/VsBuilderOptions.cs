#region License

/*
 * File: VsBuilderOptions.cs
 *
 * The MIT License
 *
 * Copyright © 2017 - 2020 AVSP Ltd
 * Copyright © 2020 Oliver Hall, Ultamation Ltd
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

namespace VsIdeBuild.VsBuilderLibrary
{
    /// <summary>
    /// Configuration options POCO for the VsBuilder build process
    /// </summary>
    public class VsBuilderOptions
    {
        /// <summary>
        /// Show the VS gui in the opened project, allow user control
        /// </summary>
        public bool Debug { get; set; }

        /// <summary>
        /// Clean is only available for the entire solution
        /// </summary>
        public bool Clean { get; set; }

        /// <summary>
        /// Perform additional checks to ensure the Crestron SDK plugin loaded and completed
        /// </summary>
        public bool Crestron { get; set; }

        /// <summary>
        /// Build Solution in every available configuration
        /// </summary>
        public bool BuildAll { get; set; }

        /// <summary>
        /// Limit build to a single solution configuration, e.g. Debug, Release
        /// </summary>
        public string BuildSolutionConfiguration { get; set; }

        /// <summary>
        /// Limit build to a single project - BuildSolutionConfiguration MUST also be specified
        /// </summary>
        public string BuildProject { get; set; }

        /// <summary>
        /// Output ProjectContexts, useful to get necessary command lines for build
        /// </summary>
        public bool ShowProjectContexts { get; set; }

        /// <summary>
        /// Output ProjectOutputs, useful to get configuration information
        /// </summary>
        public bool ShowProjectOutputs { get; set; }

        /// <summary>
        /// Give detailed build output
        /// </summary>
        public bool ShowBuild { get; set; }

        /// <summary>
        /// Filepath of Visual Studio 2008 solution file
        /// </summary>
        public string Solution { get; set; }
    }
}
#region License

/*
 * File: VsBuilder.cs
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

using System;
using System.IO;
using EnvDTE;
using EnvDTE80;

namespace VsIdeBuild.VsBuilderLibrary
{
    public class VsBuilder
    {
        /// <summary>
        /// Build log message we look for to indicate sandbox failure
        /// </summary>
        private const string crestronSandboxFailureMessage = "was not prepared";
        private const string crestronPluginStart = "Preparing SIMPL # Project";
        private const string crestronPluginSuccess = "Prepared for use on a Crestron control system";

        private object visualStudio;
        private DTE dte;
        private DTE2 dte2;
        private Solution sln;
        private VsBuilderOptions options;

        /// <summary>
        /// After calling Run, Results will contain build counts and fail counts
        /// </summary>
        public VsBuilderResults Results { get; private set; }

        /// <summary>
        /// Build solution as specified in options
        /// </summary>
        /// <param name="options">Build configuration options</param>
        /// <returns></returns>
        public int Run(VsBuilderOptions options)
        {
            int returnValue = 0;

            this.options = options;
            this.Results = new VsBuilderResults();

            Console.WriteLine("Opening Visual Studio 2008...");
            OpenVS();

            dte.SuppressUI = !options.Debug;
            dte.UserControl = options.Debug;

            // Resolve the solution absolute filepath
            string absSolutionFilePath = ResolveSolutionName(options.Solution);

            if (!File.Exists(absSolutionFilePath))
            {
                Console.WriteLine("Solution file not found");
                return 1;
            }

            Console.WriteLine("Opening Solution...");
            if (!OpenSolution(absSolutionFilePath))
            {
                Console.WriteLine("Solution could not be opened");
                return 2;
            }

#if DEBUG
            Console.WriteLine("Solution.Count = " + sln.Count);

            Console.WriteLine("Projects Names...");
            foreach (Project project in dte.Solution.Projects)
            {
                Console.WriteLine("Project: " + project.Name);
            }
#endif

            if (options.ShowProjectContexts)
            {
                Console.WriteLine("Showing project contexts...");
                Console.WriteLine(GetProjectContexts());
            }

            if (options.ShowProjectOutputs)
            {
                Console.WriteLine("Showing project outputs...");
                ShowProjectOutputs();
            }

            if (options.BuildAll)
            {
                BuildAll();
            }
            else
            {
                if (options.BuildSolutionConfiguration != null)
                {
                    if (options.BuildProject != null)
                    {
                        string projUniqueName = GetProjectUniqueName(options.BuildProject);

                        if (string.IsNullOrEmpty(projUniqueName))
                        {
                            Console.WriteLine("ERROR: The specified project was not found in the solution.");
                            returnValue = 1;
                        }
                        else
                            BuildProject(options.BuildSolutionConfiguration, projUniqueName);
                    }
                    else
                    {
                        BuildSolutionConfiguration(options.BuildSolutionConfiguration);
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: neither BuildAll or BuildSolutionConfiguration was specified");
                    returnValue = 1;
                }
            }

            if (options.ShowBuild)
            {
                string buildOutput = GetOutputWindowText("Build");

                if (!string.IsNullOrEmpty(buildOutput))
                {
                    Console.WriteLine("Build Output:");
                    
                    string[] buildLines = buildOutput.Split('\n');

                    foreach (string line in buildLines)
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine("Build Output: None");
                }
            }

            Console.WriteLine("Closing Solution...");
            CloseSolution();

            Console.WriteLine("Closing Visual Studio...");
            CloseVS();

            return returnValue;
        }

        private void PostBuildChecks()
        {
            if (sln.SolutionBuild.LastBuildInfo != 0)
            {
                Console.WriteLine("ERROR: some projects failed to build!");
                Results.Failed = true;
                return;
            }

            // Crestron SDK does not report sandbox failure as a failed build, because it is in a post-build step, so detect it separately
            string buildOutput = GetOutputWindowText("Build");

            if (buildOutput != null)
            {
                if (buildOutput.IndexOf(crestronSandboxFailureMessage) != -1)
                {
                    Console.WriteLine("ERROR: Crestron sandbox failures in build!");
                    Results.Failed = true;
                }
                else if (options.Crestron && (buildOutput.IndexOf(crestronPluginStart) == -1))
                {
                    Console.WriteLine("ERROR: Crestron plugin did not run!");
                    Results.Failed = true;
                }
                else if (options.Crestron && (buildOutput.IndexOf(crestronPluginSuccess) == -1))
                {
                    Console.WriteLine("ERROR: Crestron plugin build failed!");
                    Results.Failed = true;
                }
            }
            else
            {
                if (options.Crestron)
                {
                    Console.WriteLine("ERROR: Crestron plugin output not found!");
                    Results.Failed = true;
                }
            }
        }

        private void BuildSolutionConfiguration(EnvDTE80.SolutionConfiguration2 solutionConfiguration2)
        {
            Console.WriteLine("Activating solution configuration '" + solutionConfiguration2.Name + "' platform '" + solutionConfiguration2.PlatformName + "'");
            solutionConfiguration2.Activate();

            if (options.Clean)
            {
                Console.WriteLine("Cleaning solution configuration '" + solutionConfiguration2.Name + "' platform '" + solutionConfiguration2.PlatformName + "'");
                sln.SolutionBuild.Clean(true);
                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("Building " + solutionConfiguration2.Name + ":" + solutionConfiguration2.PlatformName);
            sln.SolutionBuild.Build(true);
            System.Threading.Thread.Sleep(1000);

            PostBuildChecks();
        }

        /// <summary>
        /// Return the unique name for the given project name
        /// This is required for single project builds
        /// </summary>
        /// <param name="projectName">The common project name</param>
        /// <returns>The corresponding unique project name, or the empty string if not found.</returns>
        private string GetProjectUniqueName(string projectName)
        {
#if DEBUG
            Console.WriteLine("Looking for project: {0}", projectName);
#endif
            foreach (Project project in dte.Solution.Projects)
            {
                string uniqueName = GetProjectUniqueName(projectName, project);
                if (!string.IsNullOrEmpty(uniqueName))
                    return uniqueName;
            }
            return string.Empty;
        }

        /// <summary>
        /// This is a recursive form of GetProjectUniqueName which checks for sub projects - which is encountered when projects are organised into folders!
        /// </summary>
        /// <param name="projectName">The project name to look for</param>
        /// <param name="projs">The project collection to search, recursively</param>
        /// <returns>A project unique name or the empty string</returns>
        private string GetProjectUniqueName(string projectName, Project project)
        {
#if DEBUG
            Console.WriteLine("Checking project: {0} => {1}", project.Name, project.UniqueName);
#endif
            if (project.Name.ToLower() == projectName.ToLower())
                return project.UniqueName;
            else
            {
                if ((project.ProjectItems != null) && (project.ProjectItems.Count > 0))
                {
                    foreach (ProjectItem projItem in project.ProjectItems)
                    {
                        if (projItem.SubProject != null)
                        {
                            string uniqueName = GetProjectUniqueName(projectName, projItem.SubProject);
                            if (!string.IsNullOrEmpty(uniqueName))
                                return uniqueName;
#if DEBUG
                            else
                            {
                                Console.WriteLine("Skipping item...");
                            }
#endif
                        }
                    }
                }
            }           
            return string.Empty;
        }

        /// <summary>
        /// Refactored solution matching
        /// </summary>
        /// <param name="solutionConfigurationName">The solution config passed as an option</param>
        /// <returns>A matching solution config, or null if no matches</returns>
        private EnvDTE80.SolutionConfiguration2 IdentifyMatchingSolution(string solutionConfigurationName)
        {
            EnvDTE.SolutionConfigurations solutionConfigurations;

            solutionConfigurations = sln.SolutionBuild.SolutionConfigurations;

            foreach (EnvDTE80.SolutionConfiguration2 solutionConfiguration2 in solutionConfigurations)
            {
                Console.WriteLine("BuildSolutionConfiguration considering solution configuration '" + solutionConfiguration2.Name + "' platform '" + solutionConfiguration2.PlatformName + "'");

                if (solutionConfiguration2.Name == solutionConfigurationName)
                {
                    Console.WriteLine("Matches, building...");
                    return solutionConfiguration2;
                }
                else
                {
                    Console.WriteLine("Does not match, skipping");
                }
            }
            return null;
        }

        private void BuildProject(string solutionConfigurationName, string projectUniqueName)
        {
            SolutionConfiguration2 slnCfg = IdentifyMatchingSolution(solutionConfigurationName);
            if (slnCfg == null)
            {
                Console.WriteLine("No configurations matching " + solutionConfigurationName + " found.");
                return;
            }

            string buildConfig = slnCfg.Name + "|" + slnCfg.PlatformName;
            Console.WriteLine("Activating solution configuration '" + buildConfig + "'");
            slnCfg.Activate();

            if (options.Clean)
            {
                Console.WriteLine("Cleaning solution configuration '" + buildConfig + "'");
                sln.SolutionBuild.Clean(true);
                System.Threading.Thread.Sleep(1000);
            }

            Console.WriteLine("Building " + buildConfig + ":" + projectUniqueName);
            sln.SolutionBuild.BuildProject(buildConfig, projectUniqueName, true);
            System.Threading.Thread.Sleep(1000);

            PostBuildChecks();
        }

        private void BuildSolutionConfiguration(string solutionConfigurationName)
        {
            SolutionConfiguration2 slnCfg = IdentifyMatchingSolution(solutionConfigurationName);
            if (slnCfg == null)
            {
                Console.WriteLine("No configurations matching " + solutionConfigurationName + " found.");
                return;
            }

            BuildSolutionConfiguration(slnCfg);
        }

        private void BuildAll()
        {
            EnvDTE.SolutionConfigurations solutionConfigurations;

            solutionConfigurations = sln.SolutionBuild.SolutionConfigurations;

            foreach (EnvDTE80.SolutionConfiguration2 solutionConfiguration2 in solutionConfigurations)
            {
                Console.WriteLine("BuildAll starting solution configuration '" + solutionConfiguration2.Name + "' platform '" + solutionConfiguration2.PlatformName + "'");
                BuildSolutionConfiguration(solutionConfiguration2);
            }
        }

        private void SaveAllOutputWindowPanes(string basePath, string projectName)
        {
            OutputWindow outputWindow = dte2.ToolWindows.OutputWindow;
            OutputWindowPanes panes = outputWindow.OutputWindowPanes;

            foreach (OutputWindowPane pane in panes)
            {
                string filename = Path.Combine(basePath, "build." + projectName + "-" + pane.Name + ".log");

                string text = GetOutputWindowPaneText(pane);
                File.WriteAllText(filename, text);
            }
        }

        private string GetOutputWindowPaneText(OutputWindowPane outputWindowPane)
        {
            TextDocument doc = outputWindowPane.TextDocument;
            TextSelection sel = doc.Selection;

            sel.SelectAll();
            string txt = sel.Text;

            return txt;
        }

        /// <summary>
        /// Get the full text from an output pane
        /// </summary>
        /// <param name="fromPane"></param>
        /// <returns>text from chosen output pane or null if pane does not exist or an error occurs</returns>
        private string GetOutputWindowText(string fromPane)
        {
            try
            {
                OutputWindow outputWindow = dte2.ToolWindows.OutputWindow;
                OutputWindowPane outputWindowPane = outputWindow.OutputWindowPanes.Item(fromPane);

                return GetOutputWindowPaneText(outputWindowPane);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ChangeProjectContexts(EnvDTE.Project project, string configurationName)
        {
            EnvDTE.SolutionConfigurations solutionConfigurations;

            solutionConfigurations = sln.SolutionBuild.SolutionConfigurations;

            foreach (EnvDTE80.SolutionConfiguration2 solutionConfiguration2 in solutionConfigurations)
            {
                foreach (EnvDTE.SolutionContext solutionContext in solutionConfiguration2.SolutionContexts)
                {
                    if (solutionContext.ProjectName == project.UniqueName)
                    {
                        solutionContext.ConfigurationName = configurationName;
                    }
                }
            }
        }

        private void ShowProjectOutputs()
        {
            foreach (Project project in dte.Solution.Projects)
            {
                Console.WriteLine("Project: " + project.Name);

                var dir = System.IO.Path.Combine(
                                    project.FullName,
                                    project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString());

                foreach (Property prop in project.ConfigurationManager.ActiveConfiguration.Properties)
                {
                    Console.WriteLine("  - " + prop.Name + " = " + prop.Value);
                }

                string outputFileName = null;

                try
                {
                    // and combine it with the OutputFilename to get the assembly
                    // or skip this and grab all files in the output directory
                    outputFileName = System.IO.Path.Combine(
                                        dir,
                                        project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputFilename").Value.ToString());
                }
                catch (ArgumentException)
                {
                    // projects in VS2008 do not seem to define this property, oh well
                    outputFileName = System.IO.Path.Combine(dir, "???.???");
                }

                Console.WriteLine(outputFileName);
            }
        }

        private string GetProjectContexts()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            EnvDTE80.Solution2 solution2;
            EnvDTE80.SolutionBuild2 solutionBuild2;
            EnvDTE.SolutionContexts solutionContexts;

            solution2 = (EnvDTE80.Solution2)sln;
            solutionBuild2 = (EnvDTE80.SolutionBuild2)solution2.SolutionBuild;

            // Solution configurations/platforms
            sb.AppendLine();
            sb.AppendLine("-----------------------------------------------");
            sb.AppendLine("Project contexts for each solution configuration/platform:");

            foreach (SolutionConfiguration2 solutionConfiguration2 in solutionBuild2.SolutionConfigurations)
            {
                sb.AppendLine();

                sb.AppendLine("   - Solution configuration: " + solutionConfiguration2.Name);
                sb.AppendLine("   - Solution platform: " + solutionConfiguration2.PlatformName);

                solutionContexts = solutionConfiguration2.SolutionContexts;

                foreach (EnvDTE.SolutionContext solutionContext in solutionContexts)
                {
                    sb.AppendLine();
                    sb.AppendLine("         Project unique name: " + solutionContext.ProjectName);
                    sb.AppendLine("         Project configuration: " + solutionContext.ConfigurationName);
                    sb.AppendLine("         Project platform: " + solutionContext.PlatformName);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        private void ChangeActiveConfigurationAndPlatform(string configurationName, string platformName)
        {
            EnvDTE80.SolutionConfiguration2 solutionConfiguration2 = null;

            solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)sln.SolutionBuild.ActiveConfiguration;

            Console.WriteLine("The old configuration was: Configuration Name: " + solutionConfiguration2.Name + ", Platform Name: " + solutionConfiguration2.PlatformName);

            foreach (EnvDTE80.SolutionConfiguration2 solConfiguration2 in sln.SolutionBuild.SolutionConfigurations)
            {
                if (solConfiguration2.Name == configurationName && solConfiguration2.PlatformName == platformName)
                {
                    solConfiguration2.Activate();
                    break;
                }
            }

            solutionConfiguration2 = (EnvDTE80.SolutionConfiguration2)sln.SolutionBuild.ActiveConfiguration;

            Console.WriteLine("The new configuration is: Configuration Name: " + solutionConfiguration2.Name + ", Platform Name: " + solutionConfiguration2.PlatformName);
        }

        public void ShowSolutionConfigurations()
        {
            EnvDTE.SolutionConfigurations solutionConfigurations;

            solutionConfigurations = sln.SolutionBuild.SolutionConfigurations;

            foreach (EnvDTE80.SolutionConfiguration2 solutionConfiguration2 in solutionConfigurations)
            {
                Console.WriteLine(" SolutionConfigurationName: " + solutionConfiguration2.Name);
                foreach (EnvDTE.SolutionContext solutionContext in solutionConfiguration2.SolutionContexts)
                {
                    Console.WriteLine("    SolutionConfigurationContext");
                    Console.WriteLine("      ProjectName = " + solutionContext.ProjectName); // will match project.UniqueName
                    Console.WriteLine("      ConfigurationName = " + solutionContext.ConfigurationName); // you can write this too
                }
            }
        }

        /// <summary>
        /// Convert the given solution name to an absolute path, and add the .sln extension
        /// </summary>
        /// <param name="solutionFile">The solution filename from the arguments</param>
        /// <returns>The absolute path to the full solution</returns>
        private string ResolveSolutionName(string solutionFile)
        {
            string absPath = Path.GetDirectoryName(Path.GetFullPath(solutionFile));
            string slnFileName = Path.GetFileNameWithoutExtension(solutionFile);
            return Path.Combine(absPath, slnFileName + ".sln");
        }

        public bool OpenSolution(string solutionFile)
        {
            sln = dte.Solution;
            sln.Open(solutionFile);
#if DEBUG
            Console.WriteLine("sln.IsOpen = " + sln.IsOpen);
#endif
            return sln.IsOpen;
        }

        public void CloseSolution()
        {
            sln.Close(false);
        }

        /// <summary>
        /// Open Visual Studio 2008
        /// </summary>
        public void OpenVS()
        {
#if DEBUG
            Console.WriteLine("Getting Type of Visual Studio...");
#endif
            Type type = Type.GetTypeFromProgID("VisualStudio.DTE.9.0");

#if DEBUG
            Console.WriteLine("Opening Visual Studio...");
#endif
            visualStudio = Activator.CreateInstance(type, true);

            // See http://msdn.microsoft.com/en-us/library/ms228772.aspx
            MessageFilter.Register();

#if DEBUG
            Console.WriteLine("Casting to DTE...");
#endif
            dte = (DTE)visualStudio;

#if DEBUG
            Console.WriteLine("Casting to DTE2...");
#endif
            dte2 = (DTE2)visualStudio;
        }

        /// <summary>
        /// Close Visual Studio 2008
        /// </summary>
        public void CloseVS()
        {
            dte.Quit();

            MessageFilter.Revoke();
        }
    }
}
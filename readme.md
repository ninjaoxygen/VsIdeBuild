# VsIdeBuild

## About

VsIdeBuild is a command line tool to automate the Visual Studio 2008 IDE to build solutions.
This is necessary when GUI IDE plugins are required for the build process, for example when using Crestron's SDK.

## Downloads

The latest binary release is available as a nuget package from https://www.nuget.org/packages/VsIdeBuild/

Other release binaries are available from https://github.com/ninjaoxygen/VsIdeBuild/releases

## Contact

E-mail: chris@avsp.co.uk

## Copyright

VsIdeBuild is Copyright (C) 2017 - 2020 AVSP Ltd

## Contributors

Many thanks for contributions from:

Oliver Hall, Ultamation Ltd

## Command line parameters:

~~~
	-Solution
		Specify Visual Studio 2008 .sln file to work on. Relative or absolute paths are acceptable, the .sln can be omitted.

	-Clean
		Cleans before build. This does not work with BuildProject - VS2008 only supports simple cleaning of enitre solution configurations.

	-BuildAll
		Builds all solution configurations, using their project build settings for each configuration.
		
	-BuildSolutionConfiguration
		Build a single solution configuration, e.g. Debug or Release.

	-BuildProject
		Build a single project, also needs -BuildSolutionConfiguration to be specified.

	-ShowProjectContexts
		Outputs all available project names and configurations.

	-ShowProjectOutputs
		Outputs all properties of all projects in the solution

	-ShowBuild
		Returns all build output from VS2008 and the Crestron SDK plugin, sandbox failures will show up here

	-Debug
		Will not hide the Visual Studio window that is opened, will allow user interaction with that window.

	-Crestron
	  Enable checks to ensure the Crestron SDK plugin intialised and completed. Without this option, only Crestron sandbox failures will be detected.
~~~
		
## Examples
		
Build only the Debug configuration of project ConsoleApp in Sample.sln
~~~
VsIdeBuild -Solution "C:\Test\Sample.sln" -BuildSolutionConfiguration "Debug" -BuildProject "ConsoleApp"
~~~

Clean then build all configurations of all projects in Sample.sln
~~~
VsIdeBuild -Solution "C:\Test\Sample.sln" -Clean -BuildAll
~~~

## Return value

0 on success

>=1 on failure

## License

The MIT License

Copyright (C) 2017 - 2020 AVSP Ltd

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

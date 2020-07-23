@ECHO OFF

ECHO Deleting old packages...

DEL /F *.nupkg

SETLOCAL enabledelayedexpansion

ECHO Creating packages
FOR %%I IN (*.nuspec) DO (
	nuget pack "%%I"
	IF NOT "!errorlevel!"=="0" SET ERROR=Failed to nuget pack "%%~fI" && GOTO :fail
)

ECHO Success

ECHO To publish to nuget use
ECHO nuget push *.nupkg -Source https://api.nuget.org/v3/index.json

EXIT /B 0

:fail

ECHO ****** Build failed! ******
ECHO %ERROR%
EXIT /B 1

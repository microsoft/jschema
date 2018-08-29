::Build NuGet packages step
@ECHO off
SETLOCAL

set BinaryOutputDirectory=%1
set Configuration=%1
set Platform=%2

if "%BinaryOutputDirectory%" EQU "" (
set BinaryOutputDirectory=.\bld\bin\
)

if "%Configuration%" EQU "" (
set Configuration=Release
)

if "%Platform%" EQU "" (
set Platform=AnyCpu
)

set BinaryOutputDirectory=%BinaryOutputDirectory%\%Platform%_%Configuration%\
set SigningDirectory=%BinaryOutputDirectory%\..\Signing

:: Copy all multitargeted assemblies to their locations
call :CopyDllForMultitargeting Json.Schema.dll            || goto :ExitFailed
call :CopyDllForMultitargeting Json.Pointer.dll           || goto :ExitFailed
call :CopyDllForMultitargeting Json.Schema.ToDotNet.dll   || goto :ExitFailed
call :CopyDllForMultitargeting Json.Schema.Validation.dll || goto :ExitFailed

call :CopyExeForMultitargeting Json.Schema.ToDotNet.Cli   || goto :ExitFailed
call :CopyExeForMultitargeting Json.Schema.Validation.Cli || goto :ExitFailed

goto :Exit

:CopyDllForMultitargeting

xcopy /Y %SigningDirectory%\netcoreapp2.0\Microsoft.%1 %BinaryOutputDirectory%\%~n1\netstandard2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %SigningDirectory%\net461\Microsoft.%1 %BinaryOutputDirectory%\%~n1\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

Exit /B %ERRORLEVEL%

:CopyExeForMultitargeting

xcopy /Y %SigningDirectory%\netcoreapp2.0\Microsoft.%1.dll %BinaryOutputDirectory%\%1\netstandard2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %SigningDirectory%\net461\Microsoft.%1.exe %BinaryOutputDirectory%\%1\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Build NuGet packages from layout directory step failed.
exit /b 1

:Exit
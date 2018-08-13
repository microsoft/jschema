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
call :CopyFilesForMultitargeting Json.Schema.dll            || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Pointer.dll           || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Schema.ToDotNet.dll   || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Schema.Validation.dll || goto :ExitFailed

goto :Exit

:CopyFilesForMultitargeting

xcopy /Y %SigningDirectory%\netcoreapp2.0\Microsoft.%1 %BinaryOutputDirectory%\%~n1\netstandard2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

echo xcopy /Y %SigningDirectory%\net461\Microsoft.%1 %BinaryOutputDirectory%\%~n1\net461\
xcopy /Y %SigningDirectory%\net461\Microsoft.%1 %BinaryOutputDirectory%\%~n1\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)


:CopyFilesExit
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Build NuGet packages from layout directory step failed.
exit /b 1

:Exit
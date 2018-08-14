::Build NuGet packages step
@ECHO off
SETLOCAL

set BinaryOutputDirectory=%1
set Configuration=%2
set Platform=%3

if "%BinaryOutputDirectory%" EQU "" (
set BinaryOutputDirectory=%~dp0bld\bin
)

if "%Configuration%" EQU "" (
set Configuration=Release
)

if "%Platform%" EQU "" (
set Platform=AnyCPU
)

set BinaryOutputDirectory=%BinaryOutputDirectory%\%Platform%_%Configuration%\
set SigningDirectory=%BinaryOutputDirectory%\..\Signing

call :CreateDirIfNotExist %SigningDirectory%
call :CreateDirIfNotExist %SigningDirectory%\net461\
call :CreateDirIfNotExist %SigningDirectory%\netcoreapp2.0

call :CopyFilesForMultitargeting Json.Schema.dll            || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Pointer.dll           || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Schema.ToDotNet.dll   || goto :ExitFailed
call :CopyFilesForMultitargeting Json.Schema.Validation.dll || goto :ExitFailed

goto :Exit

:CopyFilesForMultitargeting
xcopy /Y %BinaryOutputDirectory%\%~n1\netstandard2.0\Microsoft.%1  %SigningDirectory%\netcoreapp2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %BinaryOutputDirectory%\%~n1\net461\Microsoft.%1  %SigningDirectory%\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

:CopyFilesExit
Exit /B %ERRORLEVEL%

:CreateDirIfNotExist
set dir=%~1
if not exist %dir% (md %dir%)
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Create layout directory step failed.
exit /b 1

:Exit
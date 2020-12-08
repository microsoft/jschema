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

:: Copy all signed assemblies to the build output directory
call :CopySignedLibraryToBuildOutputDirectory Json.Schema.dll            || goto :ExitFailed
call :CopySignedLibraryToBuildOutputDirectory Json.Pointer.dll           || goto :ExitFailed
call :CopySignedLibraryToBuildOutputDirectory Json.Schema.ToDotNet.dll   || goto :ExitFailed
call :CopySignedLibraryToBuildOutputDirectory Json.Schema.Validation.dll || goto :ExitFailed

call :CopySignedExecutableToBuildOutputDirectory Json.Schema.ToDotNet.Cli   || goto :ExitFailed
call :CopySignedExecutableToBuildOutputDirectory Json.Schema.Validation.Cli || goto :ExitFailed

goto :Exit

:CopySignedLibraryToBuildOutputDirectory

xcopy /Y %SigningDirectory%\netcoreapp2.1\Microsoft.%1 %BinaryOutputDirectory%\%~n1\netstandard2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %SigningDirectory%\net461\Microsoft.%1 %BinaryOutputDirectory%\%~n1\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)
goto :CopyFilesExit

:CopySignedExecutableToBuildOutputDirectory

xcopy /Y %SigningDirectory%\netcoreapp2.1\Microsoft.%1.dll %BinaryOutputDirectory%\%1\netstandard2.0\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %SigningDirectory%\net461\Microsoft.%1.exe %BinaryOutputDirectory%\%1\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)

:CopyFilesExit
Exit /B %ERRORLEVEL%

:ExitFailed
@echo.
@echo Build NuGet packages from layout directory step failed.
exit /b 1

:Exit
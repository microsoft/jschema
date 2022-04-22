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
call :CreateDirIfNotExist %SigningDirectory%\netcoreapp3.1

call :CopyUnsignedLibraryToSigningDirectory Json.Schema.dll            || goto :ExitFailed
call :CopyUnsignedLibraryToSigningDirectory Json.Pointer.dll           || goto :ExitFailed
call :CopyUnsignedLibraryToSigningDirectory Json.Schema.ToDotNet.dll   || goto :ExitFailed
call :CopyUnsignedLibraryToSigningDirectory Json.Schema.Validation.dll || goto :ExitFailed

call :CopyUnsignedExecutableToSigningDirectory Json.Schema.ToDotNet.Cli   || goto :ExitFailed
call :CopyUnsignedExecutableToSigningDirectory Json.Schema.Validation.Cli || goto :ExitFailed

goto :Exit

:CopyUnsignedLibraryToSigningDirectory
xcopy /Y %BinaryOutputDirectory%\%~n1\netstandard2.0\Microsoft.%1  %SigningDirectory%\netcoreapp3.1\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %BinaryOutputDirectory%\%~n1\net461\Microsoft.%1  %SigningDirectory%\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)
goto :CopyFilesExit

:CopyUnsignedExecutableToSigningDirectory
xcopy /Y %BinaryOutputDirectory%\%1\netcoreapp3.1\Microsoft.%1.dll  %SigningDirectory%\netcoreapp3.1\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed. && goto :CopyFilesExit)

xcopy /Y %BinaryOutputDirectory%\%1\net461\Microsoft.%1.exe  %SigningDirectory%\net461\
if "%ERRORLEVEL%" NEQ "0" (echo %1 assembly copy failed.)

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
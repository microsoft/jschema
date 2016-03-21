@echo off
setlocal

if exist bld rmdir /s /q bld

set Configuration=Release

REM Restore NuGet packages.
.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

REM Build solution, including NuGet packages.
msbuild /verbosity:minimal /target:rebuild src\Everything.sln /p:Configuration=%Configuration% /filelogger /fileloggerparameters:Verbosity=normal
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

REM Run tests.
set XUNIT=src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe

%XUNIT% bld\bin\Json.Schema.ToDotNet.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.ToDotNet.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

%XUNIT% bld\bin\Json.Schema.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo .
@echo SCRIPT FAILED

:Exit

endlocal && exit /b %ERRORLEVEL%
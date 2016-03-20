@echo off
setlocal

rmdir /s /q bld

msbuild /verbosity:minimal /target:rebuild src\Everything.sln
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

set XUNIT=src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe

%XUNIT% bld\bin\Json.Schema.ToDotNet.UnitTests\AnyCPU_Debug\Microsoft.Json.Schema.ToDotNet.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

%XUNIT% bld\bin\Json.Schema.UnitTests\AnyCPU_Debug\Microsoft.Json.Schema.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo .
@echo SCRIPT FAILED

:Exit

endlocal && exit /b %ERRORLEVEL%
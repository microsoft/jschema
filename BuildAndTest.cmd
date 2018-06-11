@echo off
setlocal

if exist bld rmdir /s /q bld

set Configuration=Release
set Platform=Any CPU

REM Restore NuGet packages.
.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

REM Build solution, including NuGet packages.
msbuild /verbosity:minimal /target:rebuild src\Everything.sln /p:Configuration=%Configuration%,Platform="%Platform%" /filelogger /fileloggerparameters:Verbosity=detailed
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

REM Run tests.
set XUNIT=src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe

%XUNIT% bld\bin\Json.Pointer.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Pointer.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

%XUNIT% bld\bin\Json.Schema.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

%XUNIT% bld\bin\Json.Schema.ToDotNet.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.ToDotNet.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

set JSON_SCHEMA_TEST_SUITE_PATH=..\JSON-Schema-Test-Suite
set JSON_SCHEMA_TEST_SUITE_URI=https://github.com/json-schema-org/JSON-Schema-Test-Suite
if not exist %JSON_SCHEMA_TEST_SUITE_PATH% (
git clone %JSON_SCHEMA_TEST_SUITE_URI% %JSON_SCHEMA_TEST_SUITE_PATH%
)

%XUNIT% bld\bin\Json.Schema.ValidationSuiteTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.ValidationSuiteTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

goto Exit

:ExitFailed
@echo .
@echo SCRIPT FAILED

:Exit

endlocal && exit /b %ERRORLEVEL%
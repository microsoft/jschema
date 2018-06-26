@echo off
setlocal EnableDelayedExpansion

set Configuration=Release
set SolutionFile=src\Everything.sln
set RunJsonSchemaTestSuite=false
set PackageOutputDirectory=%cd%\bld\bin\NuGet\%Configuration%

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)
if "%1" == "/run-json-schema-test-suite" (
    set RunJsonSchemaTestSuite=true&& shift && goto :NextArg
)
echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

if exist bld rmdir /s /q bld

dotnet build --no-incremental --configuration %Configuration% /fileloggerparameters:Verbosity=detailed %SolutionFile% 
if "%ERRORLEVEL%" NEQ "0" (
    echo Build failed.
    goto ExitFailed
)

if "%RunJsonSchemaTestSuite%" == "true" (
    REM To understand the reason for this, see the comment in
    REM src\Json.Schema.Validation.UnitTests\ValidationSuite.cs.
    set JSON_SCHEMA_TEST_SUITE_PATH=..\JSON-Schema-Test-Suite
    set JSON_SCHEMA_TEST_SUITE_URI=https://github.com/json-schema-org/JSON-Schema-Test-Suite
    if not exist !JSON_SCHEMA_TEST_SUITE_PATH! (
        echo Cloning the offical JSON schema test suite...
        git clone !JSON_SCHEMA_TEST_SUITE_URI! !JSON_SCHEMA_TEST_SUITE_PATH!
    )
)

for %%i in (Json.Pointer, Json.Schema, Json.Schema.ToDotNet, Json.Schema.Validation) DO (
    dotnet test --no-build --no-restore src\%%i.UnitTests\%%i.UnitTests.csproj
    if "%ERRORLEVEL%" NEQ "0" (
        echo %%i unit tests failed.
        goto ExitFailed
    )
)

dotnet pack --no-build --no-restore --include-symbols -o %PackageOutputDirectory% %SolutionFile%
if "%ERRORLEVEL%" NEQ "0" (
    echo Package creation failed.
    goto ExitFailed
)

echo.
echo SUCCESS!

goto Exit

:ExitFailed
echo.
echo SCRIPT FAILED.

:Exit
endlocal && exit /b %ERRORLEVEL%
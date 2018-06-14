@echo off
setlocal

set Configuration=Release
set SolutionFile=src\Everything.sln

:NextArg
if "%1" == "" goto :EndArgs
if "%1" == "/config" (
    if not "%2" == "Debug" if not "%2" == "Release" echo error: /config must be either Debug or Release && goto :ExitFailed
    set Configuration=%2&& shift && shift && goto :NextArg
)

echo Unrecognized option "%1" && goto :ExitFailed

:EndArgs

if exist bld rmdir /s /q bld

dotnet build --no-incremental --configuration %Configuration% %SolutionFile%
if "%ERRORLEVEL%" NEQ "0" (
    echo Build failed.
    goto ExitFailed
)

dotnet test --no-build --no-restore src\Json.Pointer.UnitTests\Json.Pointer.UnitTests.csproj
if "%ERRORLEVEL%" NEQ "0" (
    echo Json.Pointer unit tests failed.
    goto ExitFailed
)

dotnet test --no-build --no-restore src\Json.Schema.UnitTests\Json.Schema.UnitTests.csproj
if "%ERRORLEVEL%" NEQ "0" (
    echo Json.Schema unit tests failed.
    goto ExitFailed
)

dotnet test --no-build --no-restore src\Json.Schema.ToDotNet.UnitTests\Json.Schema.ToDotNet.UnitTests.csproj
if "%ERRORLEVEL%" NEQ "0" (
    echo Json.Schema.ToDotNet unit tests failed.
    goto ExitFailed
)

dotnet pack --no-build --no-restore --include-symbols %SolutionFile%
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
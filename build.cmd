@echo off
cd %~dp0

SETLOCAL
SET CACHED_NUGET=%LocalAppData%\NuGet\NuGet.exe

IF EXIST %CACHED_NUGET% goto copynuget
echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%CACHED_NUGET%'"

:copynuget
IF EXIST .nuget\nuget.exe goto restore
md .nuget
copy %CACHED_NUGET% .nuget\nuget.exe > nul

:restore
IF EXIST packages\KoreBuild goto run
.nuget\NuGet.exe install KoreBuild -ExcludeVersion -o packages -nocache -pre
.nuget\NuGet.exe install Sake -ExcludeVersion -Source https://www.nuget.org/api/v2/ -Out packages

IF "%1" == "rebuild-package" goto run

IF "%SKIP_DNX_INSTALL%"=="1" (
    REM On the CI, don't upgrade since the previous installed DNVM is already there.
    CALL packages\KoreBuild\build\dnvm use default -runtime CLR -arch x86
) ELSE (
    CALL packages\KoreBuild\build\dnvm upgrade -runtime CLR -arch x86
)

:run
packages\Sake\tools\Sake.exe -I packages\KoreBuild\build -f makefile.shade %*

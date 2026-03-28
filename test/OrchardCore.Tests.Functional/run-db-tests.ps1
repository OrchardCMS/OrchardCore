# Usage:
#   .\run-db-tests.ps1 postgres   # Run CMS tests against PostgreSQL
#   .\run-db-tests.ps1 mysql      # Run CMS tests against MySQL
#   .\run-db-tests.ps1 mssql      # Run CMS tests against SQL Server
#   .\run-db-tests.ps1 sqlite     # Run CMS tests against SQLite (no container)
#   .\run-db-tests.ps1 all        # Run against all databases sequentially
#   .\run-db-tests.ps1 cleanup    # Remove all test containers

param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet('postgres','mysql','mssql','sqlite','all','cleanup')]
    [string]$Target
)

$ErrorActionPreference = 'Stop'

# Force English locale so test assertions match regardless of host language.
$env:LANG    = 'en_US.UTF-8'
$env:LC_ALL  = 'en_US.UTF-8'

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$Project   = Join-Path $ScriptDir 'OrchardCore.Tests.Functional.csproj'

function Invoke-Cleanup {
    Write-Host "Cleaning up test containers..."
    docker rm -f oc-test-postgres oc-test-mysql oc-test-mssql 2>$null
}

function Invoke-Build {
    Write-Host "Building..."
    dotnet build -c Release $Project

    $BuildDir     = Join-Path $ScriptDir 'bin\Release\net10.0'
    $PlaywrightDll = Join-Path $BuildDir 'Microsoft.Playwright.dll'
    if (Test-Path $PlaywrightDll) {
        Write-Host "Installing Playwright browsers..."
        $RuntimeConfig = Join-Path $BuildDir 'OrchardCore.Tests.Functional.runtimeconfig.json'
        dotnet exec --runtimeconfig $RuntimeConfig $PlaywrightDll install chromium
    }
}

function Invoke-Tests {
    param([string]$ConnString, [string]$Provider)

    Write-Host ""
    Write-Host "========================================="
    Write-Host "Running CMS tests with $Provider"
    Write-Host "========================================="

    $env:ASPNETCORE_ENVIRONMENT             = 'Production'
    $env:OrchardCore__ConnectionString      = $ConnString
    $env:OrchardCore__DatabaseProvider      = $Provider
    try {
        dotnet test --project $Project -c Release --no-build --filter-class "*Cms*"
    } finally {
        Remove-Item Env:\ASPNETCORE_ENVIRONMENT             -ErrorAction SilentlyContinue
        Remove-Item Env:\OrchardCore__ConnectionString      -ErrorAction SilentlyContinue
        Remove-Item Env:\OrchardCore__DatabaseProvider      -ErrorAction SilentlyContinue
    }
}

function Invoke-SqliteTests {
    Write-Host ""
    Write-Host "========================================="
    Write-Host "Running CMS tests with SQLite"
    Write-Host "========================================="

    $env:ASPNETCORE_ENVIRONMENT = 'Production'
    try {
        dotnet test --project $Project -c Release --no-build --filter-class "*Cms*"
    } finally {
        Remove-Item Env:\ASPNETCORE_ENVIRONMENT -ErrorAction SilentlyContinue
    }
}

function Start-Postgres {
    docker rm -f oc-test-postgres 2>$null
    Write-Host "Starting PostgreSQL..."
    docker run -d --name oc-test-postgres -p 5432:5432 `
        -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=admin -e POSTGRES_DB=app `
        --health-cmd="pg_isready" --health-interval=2s --health-timeout=5s --health-retries=10 `
        postgres:11

    Write-Host "Waiting for PostgreSQL to be ready..."
    while ((docker inspect --format='{{.State.Health.Status}}' oc-test-postgres 2>$null) -notmatch 'healthy') {
        Start-Sleep -Seconds 1
    }
    Write-Host "PostgreSQL is ready."
}

function Start-MySQL {
    docker rm -f oc-test-mysql 2>$null
    Write-Host "Starting MySQL..."
    docker run -d --name oc-test-mysql -p 3306:3306 `
        -e MYSQL_DATABASE=test -e MYSQL_ROOT_PASSWORD=test123 `
        --health-cmd="mysqladmin ping -h localhost" --health-interval=2s --health-timeout=5s --health-retries=15 `
        mysql:8

    Write-Host "Waiting for MySQL to be ready..."
    while ((docker inspect --format='{{.State.Health.Status}}' oc-test-mysql 2>$null) -notmatch 'healthy') {
        Start-Sleep -Seconds 1
    }
    Write-Host "MySQL is ready."
}

function Start-MsSql {
    docker rm -f oc-test-mssql 2>$null
    Write-Host "Starting SQL Server..."
    docker run -d --name oc-test-mssql -p 1433:1433 `
        -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=Password12!" `
        mcr.microsoft.com/mssql/server:2019-latest

    Write-Host "Waiting for SQL Server to be ready..."
    Start-Sleep -Seconds 10
    Write-Host "SQL Server is ready."
}

switch ($Target) {
    'postgres' {
        Invoke-Build
        Start-Postgres
        Invoke-Tests 'User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=app;' 'Postgres'
    }
    'mysql' {
        Invoke-Build
        Start-MySQL
        Invoke-Tests 'server=localhost;uid=root;pwd=test123;database=test' 'MySql'
    }
    'mssql' {
        Invoke-Build
        Start-MsSql
        Invoke-Tests 'Server=localhost;Database=tempdb;User Id=sa;Password=Password12!;Encrypt=False' 'SqlConnection'
    }
    'sqlite' {
        Invoke-Build
        Invoke-SqliteTests
    }
    'all' {
        Invoke-Build
        Invoke-SqliteTests
        Start-Postgres
        Invoke-Tests 'User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=app;' 'Postgres'
        Start-MySQL
        Invoke-Tests 'server=localhost;uid=root;pwd=test123;database=test' 'MySql'
        Start-MsSql
        Invoke-Tests 'Server=localhost;Database=tempdb;User Id=sa;Password=Password12!;Encrypt=False' 'SqlConnection'
    }
    'cleanup' {
        Invoke-Cleanup
    }
}

#!/usr/bin/env bash
set -euo pipefail

# Force English locale so test assertions match regardless of host language.
export LANG=en_US.UTF-8
export LC_ALL=en_US.UTF-8

# Usage:
#   ./run-db-tests.sh postgres   # Run CMS tests against PostgreSQL
#   ./run-db-tests.sh mysql      # Run CMS tests against MySQL
#   ./run-db-tests.sh mssql      # Run CMS tests against SQL Server
#   ./run-db-tests.sh sqlite     # Run CMS tests against SQLite (no container)
#   ./run-db-tests.sh all        # Run against all databases sequentially
#   ./run-db-tests.sh cleanup    # Remove all test containers

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$SCRIPT_DIR/OrchardCore.Tests.Functional.csproj"

cleanup() {
    echo "Cleaning up test containers..."
    docker rm -f oc-test-postgres oc-test-mysql oc-test-mssql 2>/dev/null || true
}

build() {
    echo "Building..."
    dotnet build -c Release "$PROJECT"

    local build_dir="$SCRIPT_DIR/bin/Release/net10.0"
    local playwright_dll="$build_dir/Microsoft.Playwright.dll"
    if [ -f "$playwright_dll" ]; then
        echo "Installing Playwright browsers..."
        dotnet exec --runtimeconfig "$build_dir/OrchardCore.Tests.Functional.runtimeconfig.json" "$playwright_dll" install chromium
    fi
}

run_tests() {
    local conn="$1"
    local provider="$2"

    echo ""
    echo "========================================="
    echo "Running CMS tests with $provider"
    echo "========================================="

    ASPNETCORE_ENVIRONMENT=Production \
    OrchardCore__ConnectionString="$conn" \
    OrchardCore__DatabaseProvider="$provider" \
        dotnet test --project "$PROJECT" -c Release --no-build --filter-class "*Cms*"
}

run_sqlite() {
    echo ""
    echo "========================================="
    echo "Running CMS tests with SQLite"
    echo "========================================="

    ASPNETCORE_ENVIRONMENT=Production \
        dotnet test --project "$PROJECT" -c Release --no-build --filter-class "*Cms*"
}

start_postgres() {
    docker rm -f oc-test-postgres 2>/dev/null || true
    echo "Starting PostgreSQL..."
    docker run -d --name oc-test-postgres -p 5432:5432 \
        -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=admin -e POSTGRES_DB=app \
        --health-cmd="pg_isready" --health-interval=2s --health-timeout=5s --health-retries=10 \
        postgres:11

    echo "Waiting for PostgreSQL to be ready..."
    until docker inspect --format='{{.State.Health.Status}}' oc-test-postgres 2>/dev/null | grep -q healthy; do
        sleep 1
    done
    echo "PostgreSQL is ready."
}

start_mysql() {
    docker rm -f oc-test-mysql 2>/dev/null || true
    echo "Starting MySQL..."
    docker run -d --name oc-test-mysql -p 3306:3306 \
        -e MYSQL_DATABASE=test -e MYSQL_ROOT_PASSWORD=test123 \
        --health-cmd="mysqladmin ping -h localhost" --health-interval=2s --health-timeout=5s --health-retries=15 \
        mysql:8

    echo "Waiting for MySQL to be ready..."
    until docker inspect --format='{{.State.Health.Status}}' oc-test-mysql 2>/dev/null | grep -q healthy; do
        sleep 1
    done
    echo "MySQL is ready."
}

start_mssql() {
    docker rm -f oc-test-mssql 2>/dev/null || true
    echo "Starting SQL Server..."
    docker run -d --name oc-test-mssql -p 1433:1433 \
        -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Password12!' \
        mcr.microsoft.com/mssql/server:2019-latest

    echo "Waiting for SQL Server to be ready..."
    sleep 10
    echo "SQL Server is ready."
}

case "${1:-}" in
    postgres)
        build
        start_postgres
        run_tests "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=app;" "Postgres"
        ;;
    mysql)
        build
        start_mysql
        run_tests "server=localhost;uid=root;pwd=test123;database=test" "MySql"
        ;;
    mssql)
        build
        start_mssql
        run_tests "Server=localhost;Database=tempdb;User Id=sa;Password=Password12!;Encrypt=False" "SqlConnection"
        ;;
    sqlite)
        build
        run_sqlite
        ;;
    all)
        build
        run_sqlite
        start_postgres
        run_tests "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=app;" "Postgres"
        start_mysql
        run_tests "server=localhost;uid=root;pwd=test123;database=test" "MySql"
        start_mssql
        run_tests "Server=localhost;Database=tempdb;User Id=sa;Password=Password12!;Encrypt=False" "SqlConnection"
        ;;
    cleanup)
        cleanup
        ;;
    *)
        echo "Usage: $0 {postgres|mysql|mssql|sqlite|all|cleanup}"
        exit 1
        ;;
esac

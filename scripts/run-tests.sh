#!/usr/bin/env bash

echo ""
echo "Run tests..."
echo ""

for test in ./test/*/
do
    echo "Testing $test"
    pushd "$test"
    dotnet test --framework netcoreapp1.0
    popd
done
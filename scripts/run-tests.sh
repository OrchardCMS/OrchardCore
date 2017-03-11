#!/usr/bin/env bash

echo ""
echo "Run tests..."
echo ""

for test in ./test/*/
do
    echo "Testing $test"
    pushd "$test"
    dotnet test
    popd
done
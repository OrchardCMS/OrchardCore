#!/bin/bash
echo "Deleting all bin and obj folders..."

find . -type d \( -name bin -o -name obj \) | while read -r dir; do
    echo "Deleting: $dir"
    rm -rf "$dir"
done

echo "Done."

#!/bin/bash
case $1 in
    "review-all")
        echo "Reviewing all C# scripts..."
        find Assets -name "*.cs" -print0 | xargs -0 -I {} claude "Review Unity script:" {}
        ;;
    "performance")
        claude "Analyze Unity project performance issues và optimization suggestions"
        ;;
    "architecture")
        claude "Review Unity project architecture và design patterns"
        ;;
    "mobile-check")
        find Assets -name "*.cs" -exec grep -l "Update\|FixedUpdate\|LateUpdate" {} \; | xargs claude "Check Update methods performance cho mobile:"
        ;;
    "memory-check")
        find Assets -name "*.cs" -exec grep -l "Instantiate\|Destroy\|new " {} \; | xargs claude "Check memory management trong scripts:"
        ;;
    "docs")
        for file in Assets/Scripts/*.cs; do
            [ -f "$file" ] && claude "Generate XML documentation cho:" < "$file" > "Docs/$(basename "$file" .cs).md"
        done
        ;;
esac

#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
results="$root/coverage/results"
report="$root/coverage/report"

if ! command -v reportgenerator >/dev/null 2>&1; then
    dotnet tool install -g dotnet-reportgenerator-globaltool
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

rm -rf "$root/coverage"

dotnet test "$root/tests/TrueCode.Finance.Application.Tests" \
    --collect:"XPlat Code Coverage" \
    --results-directory "$results"

reportgenerator \
    -reports:"$results/**/coverage.cobertura.xml" \
    -targetdir:"$report" \
    -reporttypes:"Html;TextSummary"

cat "$report/Summary.txt"

if command -v xdg-open >/dev/null 2>&1; then
    xdg-open "$report/index.html" >/dev/null 2>&1 &
elif command -v open >/dev/null 2>&1; then
    open "$report/index.html"
fi

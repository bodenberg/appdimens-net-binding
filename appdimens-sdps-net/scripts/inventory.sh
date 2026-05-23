#!/usr/bin/env bash
# Generate module inventory and SBOM summary for appdimens-net audit.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
REPO_ROOT="$(cd "$ROOT/.." && pwd)"
OUT="${1:-$ROOT/docs/BASELINE-INVENTORY.txt}"

mkdir -p "$(dirname "$OUT")"

{
  echo "=== AppDimens Inventory ==="
  echo "Date: $(date -u +%Y-%m-%dT%H:%M:%SZ)"
  echo "Git: $(git -C "$REPO_ROOT" rev-parse HEAD 2>/dev/null || echo 'n/a')"
  echo "SDK: $(dotnet --version 2>/dev/null || echo 'n/a')"
  echo ""
  echo "=== MAUI Projects ==="
  find "$ROOT/src" "$ROOT/tests" "$ROOT/benchmarks" "$ROOT/samples" -name '*.csproj' 2>/dev/null | sort
  echo ""
  echo "=== Binding Projects ==="
  find "$REPO_ROOT" -path '*/obj' -prune -o -path '*/bin' -prune -o -name '*.csproj' -print 2>/dev/null \
    | grep -E 'net-binding' | sort
  echo ""
  echo "=== Scripts ==="
  find "$ROOT/scripts" "$REPO_ROOT" -maxdepth 3 -name '*.sh' -o -name '*.py' 2>/dev/null | grep -E 'scripts/' | sort
  echo ""
  echo "=== Documentation ==="
  find "$REPO_ROOT" -maxdepth 3 -name 'README.md' -o -name 'NUGET-PUBLISH.md' -o -path '*/docs/*.md' 2>/dev/null | sort
} > "$OUT"

echo "Wrote $OUT"

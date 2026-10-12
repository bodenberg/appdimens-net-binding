#!/usr/bin/env bash
# Pack (and optionally publish) every AppDimens.Net.* package: core, strategy satellites,
# meta-package, MAUI integration and the WPF / Windows Forms / WinUI 3 adapters.
# Usage:
#   ./scripts/pack-nuget.sh                         # test + pack into artifacts/nuget
#   NUGET_API_KEY=xxx ./scripts/pack-nuget.sh --push
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

PUSH=false
for arg in "$@"; do
  case "$arg" in
    --push) PUSH=true ;;
    -h|--help) echo "Usage: $0 [--push]"; exit 0 ;;
    *) echo "Unknown argument: $arg" >&2; exit 1 ;;
  esac
done

VERSION="$(sed -n 's:.*<Version>\([^<]*\)</Version>.*:\1:p' Directory.Build.props | head -1)"
OUT_DIR="$ROOT/artifacts/nuget"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"
mkdir -p "$OUT_DIR"

echo "==> AppDimens.Net v${VERSION} (SDK $(dotnet --version))"
echo "==> Tests..."
dotnet test tests/AppDimens.Net.Tests/AppDimens.Net.Tests.csproj -c Release

PKGS=()
for proj in src/*/*.csproj; do
  name="$(basename "$proj" .csproj)"
  echo "==> dotnet pack $name"
  dotnet pack "$proj" -c Release -o "$OUT_DIR" -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
  pkg="$OUT_DIR/${name}.${VERSION}.nupkg"
  [[ -f "$pkg" ]] || { echo "Package not found: $pkg" >&2; exit 1; }
  grep -qF "README.md" < <(unzip -Z1 "$pkg") || { echo "README missing from $pkg" >&2; exit 1; }
  PKGS+=("$pkg")
done

echo "==> ${#PKGS[@]} packages in $OUT_DIR"

if [[ "$PUSH" == true ]]; then
  [[ -n "${NUGET_API_KEY:-}" ]] || { echo "Set NUGET_API_KEY for --push" >&2; exit 1; }
  for pkg in "${PKGS[@]}"; do
    dotnet nuget push "$pkg" --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE" --skip-duplicate
  done
fi

#!/usr/bin/env bash
# Pack and (optionally) publish Bodenberg.AppDimens.Maui.Sdps to NuGet.org.
# Usage:
#   ./scripts/publish-nuget.sh           # pack + verification only
#   NUGET_API_KEY=xxx ./scripts/publish-nuget.sh --push

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

PUSH=false
for arg in "$@"; do
  case "$arg" in
    --push) PUSH=true ;;
    -h|--help)
      echo "Usage: $0 [--push]"
      echo "  --push   upload .nupkg and .snupkg to https://api.nuget.org/v3/index.json (requires NUGET_API_KEY)"
      exit 0
      ;;
    *)
      echo "Unknown argument: $arg" >&2
      exit 1
      ;;
  esac
done

VERSION="$(sed -n 's:.*<Version>\([^<]*\)</Version>.*:\1:p' Directory.Build.props | head -1)"
if [[ -z "$VERSION" ]]; then
  echo "Could not read <Version> from Directory.Build.props" >&2
  exit 1
fi

PKG_ID="Bodenberg.AppDimens.Maui.Sdps"
OUT_DIR="$ROOT/artifacts/nuget"
GENERATED="$ROOT/src/AppDimens.Maui.Resources/Generated"
SDK_PROJ="$ROOT/src/AppDimens.Maui.Sdk/AppDimens.Maui.Sdk.csproj"
NUPKG="$OUT_DIR/${PKG_ID}.${VERSION}.nupkg"
SNUPKG="$OUT_DIR/${PKG_ID}.${VERSION}.snupkg"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"

echo "==> Package: ${PKG_ID} v${VERSION}"
echo "==> SDK: $(dotnet --version)"

if [[ ! -f "$GENERATED/buckets.json" ]]; then
  echo "==> Generating buckets (generate-dimens.py)..."
  python3 "$ROOT/scripts/generate-dimens.py"
fi

if [[ ! -f "$GENERATED/buckets.json" ]]; then
  echo "Failed: $GENERATED/buckets.json missing after generation." >&2
  exit 1
fi

echo "==> Tests (Release)..."
dotnet test "$ROOT/tests/AppDimens.Maui.Tests/AppDimens.Maui.Tests.csproj" -c Release --no-restore 2>/dev/null \
  || dotnet test "$ROOT/tests/AppDimens.Maui.Tests/AppDimens.Maui.Tests.csproj" -c Release

mkdir -p "$OUT_DIR"
echo "==> dotnet pack..."
dotnet pack "$SDK_PROJ" -c Release -o "$OUT_DIR" \
  /p:IncludeSymbols=true \
  /p:SymbolPackageFormat=snupkg

if [[ ! -f "$NUPKG" ]]; then
  echo "Package not found: $NUPKG" >&2
  exit 1
fi

echo "==> .nupkg contents (summary):"
unzip -l "$NUPKG" | grep -E "lib/net|analyzers/|buckets\.json|README" || true
DLL_COUNT="$(unzip -l "$NUPKG" | grep -c '\.dll' || true)"
echo "    DLLs in package: ${DLL_COUNT}"

echo "==> Quick checks..."
nupkg_has() { grep -qF "$1" < <(unzip -Z1 "$NUPKG"); }
MISSING=0
for pattern in \
  "lib/net8.0/AppDimens.Maui.dll" \
  "lib/net10.0/AppDimens.Maui.dll" \
  "analyzers/dotnet/cs/AppDimens.Maui.SourceGen.dll" \
  "contentFiles/any/any/Generated/buckets.json" \
  "contentFiles/any/any/Generated/Dimens.300.xaml" \
  "contentFiles/any/any/Generated/Dimens.Base.xaml"; do
  if ! nupkg_has "$pattern"; then
    echo "MISSING from package: $pattern" >&2
    MISSING=1
  fi
done
if [[ "$MISSING" -ne 0 ]]; then
  echo "Fix packaging before publishing (see NUGET-PUBLISH.md)." >&2
  exit 1
fi

echo "==> OK: $NUPKG"
[[ -f "$SNUPKG" ]] && echo "==> OK: $SNUPKG"

if [[ "$PUSH" == true ]]; then
  if [[ -z "${NUGET_API_KEY:-}" ]]; then
    echo "Set NUGET_API_KEY for --push" >&2
    exit 1
  fi
  echo "==> Pushing to NuGet.org..."
  # dotnet nuget push uploads the companion .snupkg automatically when it sits beside the .nupkg.
  dotnet nuget push "$NUPKG" --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE" --skip-duplicate
  echo "==> Published. Wait for indexing: https://www.nuget.org/packages/${PKG_ID}"
else
  echo "==> Pack complete (no push). To publish:"
  echo "    NUGET_API_KEY='...' $0 --push"
fi

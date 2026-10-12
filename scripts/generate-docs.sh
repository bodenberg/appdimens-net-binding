#!/usr/bin/env bash
# Regenerates the documentation of every package under docs/<PackageId>/:
#   - API.md for .NET libraries: tools/ApiDocGen (compiled assemblies + XML docs, any OS)
#   - API.md for Android bindings: tools/aar-api-doc.py (javap over the embedded AARs, needs a JDK)
#   - README.md mirrors of package READMEs that are complete guides: tools/sync-guides.py
# Hand-written guides (AppDimens.Net.* docs) are not touched.
#
# Usage: ./scripts/generate-docs.sh
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
DOCS="$ROOT/docs"
NUGET="${NUGET_PACKAGES:-$HOME/.nuget/packages}"

echo "==> Building libraries (Release)"
SDPS="$ROOT/appdimens-sdps-net"
NET="$ROOT/appdimens-net"
[[ -f "$SDPS/src/AppDimens.Maui.Resources/Generated/buckets.json" ]] || python3 "$SDPS/scripts/generate-dimens.py"
for p in "$SDPS"/src/AppDimens.Maui.Sdk "$SDPS"/src/AppDimens.Sdps.Wpf "$SDPS"/src/AppDimens.Sdps.WinForms "$SDPS"/src/AppDimens.Sdps.WinUI; do
  dotnet build "$p" -c Release -v q -nologo >/dev/null
done
for p in "$NET"/src/*/; do
  dotnet build "$p" -c Release -v q -nologo >/dev/null
done
dotnet build "$ROOT/tools/ApiDocGen" -c Release -v q -nologo >/dev/null
GEN=(dotnet "$ROOT/tools/ApiDocGen/bin/Release/net10.0/ApiDocGen.dll")

# Reference assemblies needed to resolve MAUI / WPF / WinForms / WinUI types.
SEARCH=()
for d in "$NUGET"/microsoft.maui.* "$NUGET"/microsoft.windowsdesktop.app.ref "$NUGET"/microsoft.windowsappsdk* \
         "$NUGET"/microsoft.windows.sdk.net.ref "$NUGET"/microsoft.graphics.win2d "$NUGET"/microsoft.web.webview2; do
  [[ -d "$d" ]] && SEARCH+=(--search "$d")
done

bin() { echo "$1/bin/Release/$2"; }

# ---------------------------------------------------------------- Bodenberg.AppDimens.Maui.Sdps
MAUI_ASMS=()
for a in AppDimens.Maui AppDimens.Maui.Core AppDimens.Maui.Hosting AppDimens.Maui.Extensions AppDimens.Maui.Markup \
         AppDimens.Maui.Xaml AppDimens.Maui.Converters AppDimens.Maui.Builders AppDimens.Maui.Helpers \
         AppDimens.Maui.Responsive AppDimens.Maui.Inverters; do
  MAUI_ASMS+=(--asm "$(bin "$SDPS/src/$a" net10.0)/$a.dll" --search "$(bin "$SDPS/src/$a" net10.0)")
done
"${GEN[@]}" --title Bodenberg.AppDimens.Maui.Sdps --out "$DOCS/Bodenberg.AppDimens.Maui.Sdps/API.md" "${MAUI_ASMS[@]}" "${SEARCH[@]}"

# ---------------------------------------------------------------- SDPS desktop adapters
for pair in "Wpf:net10.0-windows" "WinForms:net10.0-windows" "WinUI:net10.0-windows10.0.19041.0"; do
  a="${pair%%:*}"; tfm="${pair#*:}"
  dir="$(bin "$SDPS/src/AppDimens.Sdps.$a" "$tfm")"
  "${GEN[@]}" --title "Bodenberg.AppDimens.Sdps.$a" --out "$DOCS/Bodenberg.AppDimens.Sdps.$a/API.md" \
    --asm "$dir/AppDimens.Sdps.$a.dll" --search "$dir" "${SEARCH[@]}" \
    --intro "The engine types embedded in this package (\`AppDimensResolver\`, \`AppDimensSdps\`, \`DimenFunction\`, \`AppDimensOptions\`, …) are documented in [Bodenberg.AppDimens.Maui.Sdps API](../Bodenberg.AppDimens.Maui.Sdps/API.md)."
done

# ---------------------------------------------------------------- AppDimens.Net.*
for proj in "$NET"/src/*/; do
  name="$(basename "$proj")"
  [[ "$name" == "AppDimens.Net.Sdk" ]] && continue   # meta-package, no public types
  tfm=net10.0
  case "$name" in
    AppDimens.Net.Wpf|AppDimens.Net.WinForms) tfm=net10.0-windows ;;
    AppDimens.Net.WinUI) tfm=net10.0-windows10.0.19041.0 ;;
  esac
  dir="$(bin "$proj" "$tfm")"
  "${GEN[@]}" --title "$name" --out "$DOCS/$name/API.md" --asm "$dir/$name.dll" --search "$dir" "${SEARCH[@]}"
done

# ---------------------------------------------------------------- Android bindings
python3 tools/aar-api-doc.py --title Bodenberg.AppDimens.Sdps --out "$DOCS/Bodenberg.AppDimens.Sdps/API.md" \
  appdimens-sdps-net-binding/AppDimens.Sdps.Binding/Jars/*.aar
python3 tools/aar-api-doc.py --title Bodenberg.AppDimens.Ssps --out "$DOCS/Bodenberg.AppDimens.Ssps/API.md" \
  appdimens-ssps-net-binding/AppDimens.Ssps.Binding/Jars/*.aar
python3 tools/aar-api-doc.py --title Bodenberg.AppDimens.Dynamic --out "$DOCS/Bodenberg.AppDimens.Dynamic/API.md" \
  appdimens-dynamic-net-binding/AppDimens.Dynamic.Binding/Jars/*.aar

python3 tools/sync-guides.py

echo "==> Documentation written to $DOCS/"

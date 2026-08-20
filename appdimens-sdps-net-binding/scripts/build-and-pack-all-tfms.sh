#!/usr/bin/env bash
# Build each Android TFM with a matching .NET SDK, then merge into one multi-TFM nupkg.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
ANDROID_SDK="${AndroidSdkDirectory:-${ANDROID_SDK_ROOT:-${ANDROID_HOME:-$HOME/Android/Sdk}}}"
STAGING="${TMPDIR:-/tmp}/sdps-nupkg-merge-$$"
mkdir -p "$STAGING"
trap 'rm -rf "$STAGING"' EXIT

declare -a BUILDS=(
  "8.0.424|net8.0-android34.0"
  "9.0.317|net9.0-android35.0"
  "10.0.400|net10.0-android36.0"
  "11.0.100-preview.7.26381.103|net11.0-android37.0"
)

BUILT_TFMS=()
for entry in "${BUILDS[@]}"; do
  IFS='|' read -r sdk tfm <<<"$entry"
  if [[ ! -d "${DOTNET_ROOT:-$HOME/.dotnet}/sdk/$sdk" ]] && [[ ! -d "/usr/share/dotnet/sdk/$sdk" ]]; then
    echo "SKIP $tfm (SDK $sdk not installed)"
    continue
  fi
  printf '%s\n' "{\"sdk\":{\"version\":\"$sdk\",\"rollForward\":\"disable\"}}" > global.json
  echo "==== Pack $tfm (SDK $sdk) ===="
  out="$STAGING/out-$tfm"
  mkdir -p "$out"
  dotnet pack AppDimens.Sdps.Binding/AppDimens.Sdps.Binding.csproj -c Release \
    -p:TargetFrameworks="$tfm" -p:TargetFramework="$tfm" \
    -p:AndroidSdkDirectory="$ANDROID_SDK" \
    -o "$out"
  BUILT_TFMS+=("$tfm")
done

if [[ ${#BUILT_TFMS[@]} -eq 0 ]]; then
  echo "No TFMs built" >&2
  exit 1
fi

python3 - "$STAGING" "$ROOT/AppDimens.Sdps.Binding/bin/Release" "${BUILT_TFMS[@]}" <<'PY'
import sys, zipfile
from pathlib import Path

staging = Path(sys.argv[1])
out_dir = Path(sys.argv[2])
tfms = sys.argv[3:]
out_dir.mkdir(parents=True, exist_ok=True)

# Prefer net10 base nuspec when present
base_tfm = next((t for t in ("net10.0-android36.0", "net9.0-android35.0", "net8.0-android34.0", "net11.0-android37.0") if t in tfms), tfms[0])
merged = staging / "merged"
merged.mkdir()
with zipfile.ZipFile(staging / f"out-{base_tfm}" / "Bodenberg.AppDimens.Sdps.3.6.0.nupkg") as zf:
    zf.extractall(merged)

lib = merged / "lib"
for tfm in tfms:
    with zipfile.ZipFile(staging / f"out-{tfm}" / "Bodenberg.AppDimens.Sdps.3.6.0.nupkg") as zf:
        for name in zf.namelist():
            if name.startswith("lib/") and not name.endswith("/"):
                target = merged / name
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(zf.read(name))

deps = {
    "net8.0-android34.0": [
        ("Xamarin.AndroidX.Core", "1.16.0.3"),
        ("Xamarin.AndroidX.Lifecycle.Runtime", "2.9.4"),
        ("Xamarin.AndroidX.Window", "1.4.0.1"),
        ("Xamarin.Kotlin.StdLib", "2.2.21"),
    ],
    "net9.0-android35.0": [
        ("Xamarin.AndroidX.Core", "1.17.0.2"),
        ("Xamarin.AndroidX.Lifecycle.Runtime", "2.10.0.2"),
        ("Xamarin.AndroidX.Window", "1.5.1.2"),
        ("Xamarin.Kotlin.StdLib", "2.3.21"),
    ],
    "net10.0-android36.0": [
        ("Xamarin.AndroidX.Core", "1.19.0.1"),
        ("Xamarin.AndroidX.Lifecycle.Runtime", "2.11.0.1"),
        ("Xamarin.AndroidX.Window", "1.5.1.3"),
        ("Xamarin.Kotlin.StdLib", "2.4.0.1"),
    ],
    "net11.0-android37.0": [
        ("Xamarin.AndroidX.Core", "1.19.0.1"),
        ("Xamarin.AndroidX.Lifecycle.Runtime", "2.11.0.1"),
        ("Xamarin.AndroidX.Window", "1.5.1.3"),
        ("Xamarin.Kotlin.StdLib", "2.4.0.1"),
    ],
}

groups = []
for tfm in tfms:
    lines = [f'      <group targetFramework="{tfm}">']
    for pkg, ver in deps[tfm]:
        lines.append(f'        <dependency id="{pkg}" version="{ver}" exclude="Build,Analyzers" />')
    lines.append("      </group>")
    groups.append("\n".join(lines))
dep_xml = "    <dependencies>\n" + "\n".join(groups) + "\n    </dependencies>"

nuspec = next(merged.glob("*.nuspec"))
text = nuspec.read_text()
import re
text2, n = re.subn(r"    <dependencies>.*?</dependencies>", dep_xml, text, count=1, flags=re.S)
if n != 1:
    raise SystemExit("failed to rewrite nuspec dependencies")
nuspec.write_text(text2)

out = out_dir / "Bodenberg.AppDimens.Sdps.3.6.0.nupkg"
if out.exists():
    out.unlink()
with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as zf:
    for path in sorted(merged.rglob("*")):
        if path.is_file():
            zf.write(path, path.relative_to(merged).as_posix())
print("Merged package:", out)
print("TFMs:", ", ".join(tfms))
PY

# Restore default global.json for day-to-day SDK 10 builds
printf '%s\n' '{"sdk":{"version":"10.0.400","rollForward":"latestFeature"}}' > global.json
echo "Done."

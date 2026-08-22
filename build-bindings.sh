#!/usr/bin/env bash
set -euo pipefail

DOTNET_ROOT="${DOTNET_ROOT:-/home/bodenberg/.dotnet}"
export DOTNET_ROOT
export PATH="$DOTNET_ROOT:$PATH"

# AndroidX/Kotlin packages (2.3.21) only support net9.0-android35.0 and net10.0-android36.0.
# net8.0-android34.0 was dropped in the 2.2.21 -> 2.3.21 bump, so we build the highest supported TFM.
TFMS=("net10.0-android36.0")

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

MODULES=(
  "appdimens-ssps-net-binding/AppDimens.Ssps.Binding/AppDimens.Ssps.Binding.csproj"
  "appdimens-sdps-net-binding/AppDimens.Sdps.Binding/AppDimens.Sdps.Binding.csproj"
  "appdimens-dynamic-net-binding/AppDimens.Dynamic.Binding/AppDimens.Dynamic.Binding.csproj"
)

for project_path in "${MODULES[@]}"; do
  echo "==== Building $project_path ===="
  for tfm in "${TFMS[@]}"; do
    echo "==== Build $tfm ===="
    dotnet build "$ROOT_DIR/$project_path" -c Release -f "$tfm" /m:1 -p:GeneratePackageOnBuild=false
  done
done

echo "==== All bindings built successfully ===="
echo "To produce NuGet packages, run: dotnet pack -c Release <project>"
#!/usr/bin/env bash
# Restore graph for CI / NuGet pack (libraries + tests + pack tooling; no MAUI app sample).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"

dotnet restore "$ROOT/tests/AppDimens.Maui.Tests/AppDimens.Maui.Tests.csproj"
dotnet restore "$ROOT/src/AppDimens.Maui.BuildTasks/AppDimens.Maui.BuildTasks.csproj"

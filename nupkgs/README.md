# NuGet packages

Ready-to-publish packages of every release: `nupkgs/<version>/*.nupkg` (+ `*.snupkg` symbol packages).
They are produced and committed by the **Release** workflow (`.github/workflows/release.yml`, input
`commit_packages = true`) — the exact files that go to NuGet.org. Do not edit them by hand.

Publish a version manually:

```bash
for p in nupkgs/3.6.1/*.nupkg; do
  dotnet nuget push "$p" --api-key "$NUGET_API_KEY" --source https://api.nuget.org/v3/index.json --skip-duplicate
done
```

(`dotnet nuget push` uploads the matching `.snupkg` automatically when it sits next to the `.nupkg`.)

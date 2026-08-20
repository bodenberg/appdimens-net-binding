# NuGet publish checklist

## MAUI package (`Bodenberg.AppDimens.Maui.Sdps`)

1. Update version numbers in `appdimens-sdps-net/src/*.csproj`.
2. Run `./scripts/publish-nuget.sh` from `appdimens-sdps-net`.
3. Verify the `.nupkg` in `artifacts/` and push with `dotnet nuget push`.

## Android bindings

### SDPS (`Bodenberg.AppDimens.Sdps`)

```bash
cd appdimens-sdps-net-binding
./scripts/sync-aar-from-maven.sh 3.1.7
./scripts/build-and-pack-all-tfms.sh
```

### SSPS (`Bodenberg.AppDimens.Ssps`)

```bash
cd appdimens-ssps-net-binding
./scripts/sync-aar-from-maven.sh 3.1.5
dotnet pack AppDimens.Ssps.Binding/AppDimens.Ssps.Binding.csproj -c Release
```

### Dynamic (`Bodenberg.AppDimens.Dynamic`)

```bash
cd appdimens-dynamic-net-binding
./scripts/sync-aar-from-maven.sh 3.1.5
dotnet pack AppDimens.Dynamic.Binding/AppDimens.Dynamic.Binding.csproj -c Release
```

Push each `.nupkg` to NuGet.org with `dotnet nuget push`.

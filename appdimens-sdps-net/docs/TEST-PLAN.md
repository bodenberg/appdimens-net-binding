# Test Plan

## Unit (automated — CI)

| Suite | Count | Scope |
|-------|-------|-------|
| `AppDimensTests.cs` | 16 | Scale, buckets, inverters, resolver, markup, SP, AR |
| `ExtendedTests.cs` | 25 | Layout v2, Hybrid, all inverters, converters, cache stress |

**Command:** `dotnet test tests/AppDimens.Maui.Tests -c Release`

## Integration

- `BucketRegistry.LoadFromGenerated` with real `Generated/` tree
- Resolver bucket formula vs Android (`411` bucket, `_16sdp`)

## Bindings (CI)

- `sync-aar-from-maven.sh 3.1.5` + `dotnet build -c Release` per binding solution

## Manual / E2E (pre-release)

- [ ] `AppDimens.Maui.Sample` on Android emulator — all tabs open without crash (Release APK)
- [ ] Rotate portrait ↔ landscape on **Home**, **Dimensions**, **Inverters**, **Advanced** — C# px labels change (e.g. `Hdp(48)`)
- [ ] **Benchmark** — run once, rotate — “último valor” updates; timings unchanged
- [ ] Shell **Voltar** from non-home tabs returns to Home
- [ ] Optional: iOS / Windows build of sample

Smoke script (Linux + emulator):

```bash
cd appdimens-sdps-net
bash scripts/smoke-android.sh
```

## Benchmarks (manual)

```bash
dotnet run -c Release --project benchmarks/AppDimens.Maui.Benchmarks
```

Regression threshold: document baseline in `artifacts/release/benchmarks/` (optional gate).

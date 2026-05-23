# AppDimens SDP — Parity Matrix (Android v3.1.5 → MAUI)

## Generated resources (MAUI layout v2)

| Android | MAUI |
|---------|------|
| `values-sw{N}dp/dimens.xml` | `Generated/Dimens.{N}.xaml` (axis-neutral keys `_N`, `_minusN`) |
| `values/dimens.xml` (base) | `Generated/Dimens.Base.xaml` |

Bucket selection per axis (sw/w/h) is unchanged; only file layout is merged.

## Constants (frozen)

| Constant | Value | Android source |
|----------|-------|----------------|
| `DESIGN_BASE_DP` | 300 | `AppDimensSdpsFactors.kt` |
| `REFERENCE_ASPECT_RATIO` | 1.78 | `AppDimensSdpsFactors.kt` |
| `ADJUSTMENT_SCALE` | 0.10/30 | `AppDimensSdpsFactors.kt` |
| `SENSITIVITY_DEFAULT` | 0.08/30 | `AppDimensSdpsFactors.kt` |
| `MAX_POSITIVE` | 600 | `generator-sdps.py` |
| `MAX_NEGATIVE` | 300 | `generator-sdps.py` |
| Bucket MIN/MAX/STEP | 30/5120/15 | `generator-sdps.py` |

## Formulas

| Type | Formula |
|------|---------|
| Bucket | `index × (bucketDp / 300)` |
| Continuous SDP | `(nominal / 300) × smallestDp` |
| AR adjustment | `arMultiplier × 300 / bucketDp` |

## API mapping

| Android (Kotlin) | MAUI (C#) | Short XAML | Normal XAML |
|------------------|-----------|------------|-------------|
| `Int.sdp` | `int.Sdp()` | `{sdp:16}` | `{dimen:Sdp Value=16}` |
| `Int.ssp` | `int.Ssp()` | `{ssp:18}` | `{dimen:Ssp Value=18}` |
| `Int.hdp` | `int.Hdp()` | `{hdp:48}` | `{dimen:Hdp Value=48}` |
| `Int.wdp` | `int.Wdp()` | `{wdp:120}` | `{dimen:Wdp Value=120}` |
| `Int.sdpa` | `int.Sdpa()` | `{sdpa:16}` | `{dimen:Sdpa Value=16}` |
| `Int.sdpPh` | `int.SdpPh()` | `{sdpPh:16}` | `{dimen:Sdp Value=16 Inverter=SwToPh}` |
| `Int.sdpPha` | `int.SdpPha()` | `{sdpPhA:16}` | `{dimen:Sdpa Value=16 Inverter=SwToPh}` |
| `Int.sdpLwa` | `int.SdpLwa()` | `{sdpLwA:16}` | `{dimen:Sdpa Value=16 Inverter=SwToLw}` |
| `Int.sdpRotate` | `int.SdpRotate(...)` | `{sdpRotate:30, Rotation=45, …}` | `{dimen:SdpRotate Base=30, …}` |
| `Int.scaledDp()` | `int.ScaledDp()` | `{scaled:14, Tablet=18, …}` | `{dimen:Scaled Value=14, Tablet=18, …}` |
| `DimenSdp.warmup` | `AppDimensSdps.Warmup()` | — | — |

### Facilitators (Rotate / Mode / Qualifier / Screen)

All C# helpers in `DimenFacilitators` have matching short and normal markup extensions.

| Helper (example) | Short XAML | Normal XAML |
|------------------|------------|-------------|
| `SdpRotate` | `{sdpRotate:30, Rotation=45, Orientation=Landscape}` | `{dimen:SdpRotate Base=30, Rotation=45, Orientation=Landscape}` |
| `HdpRotate` | `{hdpRotate:48, Rotation=64, …}` | `{dimen:HdpRotate …}` |
| `WdpRotate` | `{wdpRotate:120, …}` | `{dimen:WdpRotate …}` |
| `SspRotate` … `WemRotate` | `{sspRotate:…}` … `{wemRotate:…}` | `{dimen:SspRotate …}` … `{dimen:WemRotate …}` |
| `SdpMode` | `{sdpMode:16, Mode=24, UiMode=Desk}` | `{dimen:SdpMode Base=16, Mode=24, UiMode=Desk}` |
| `SspMode` … `WemMode` | `{sspMode:…}` … `{wemMode:…}` | `{dimen:SspMode …}` … `{dimen:WemMode …}` |
| `SdpQualifier` | `{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}` | `{dimen:SdpQualifier …}` |
| `SspQualifier` … `WemQualifier` | `{sspQualifier:…}` … `{wemQualifier:…}` | `{dimen:SspQualifier …}` … |
| `SdpScreen` | `{sdpScreen:16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}` | `{dimen:SdpScreen …}` |
| `SspScreen` … `WemScreen` | `{sspScreen:…}` … `{wemScreen:…}` | `{dimen:SspScreen …}` … |

**C# only (no XAML token):** `Responsive.Value(…).Screen(…)` chains beyond `{scaled:…}`; `SdpPx()` / `SspPx()`; binding converters; source-gen `Dimen._16sdp`.

**Normal syntax only:** inverters on `hdpa`/`wdpa`/`hspa`/`wspa` via `{dimen:Hdpa Value=16 Inverter=…}` (no dedicated short prefix).

## Inverters (8 + DEFAULT)

| Inverter | Landscape + SW | Portrait + SW |
|----------|----------------|---------------|
| `PhToLw` | HEIGHT→WIDTH | — |
| `PwToLh` | WIDTH→HEIGHT | — |
| `LhToPw` | — | HEIGHT→WIDTH |
| `LwToPh` | — | WIDTH→HEIGHT |
| `SwToLh` | SW→HEIGHT | — |
| `SwToLw` | SW→WIDTH | — |
| `SwToPh` | — | SW→HEIGHT |
| `SwToPw` | — | SW→WIDTH |

## Orientation

Android SDP has **no** `-land`/`-port` qualifiers. Rotation handled via:
- Metric recalculation (width/height/smallest) on `DeviceDisplay.MainDisplayInfoChanged` (automatic after `Initialize`)
- Independent bucket selection per axis
- Inverter axis switching (at resolve time)
- Conditional APIs (`sdpRotate`, `{scaled:… Landscape=…}`, `DimenScaled.Screen(Orientation)`)

**Developer setup:** `UseAppDimensSdps()` in `MauiProgram.cs`; Android `ConfigurationChanges` + `OnConfigurationChanged` → `RefreshMetricsFromDevice()` on `MainActivity` (sample). No AppDimens manifest entries.

**Cache:** resolver invalidates dimension cache on every metrics change (rotation, DPI, resize) — not only when responsive buckets are active.

**Sample UI:** `DimensSampleRefresh.WhenMetricsChange` re-applies C#-built labels/cards on `Metrics.Changed` (Home, Dimensions, Inverters, Advanced, Benchmark).

**XAML:** markup is load-time; resolver metrics update on rotation, but existing `Padding`/`FontSize` values need code refresh or page reload to change on screen. See README § [Screen rotation and resize](../README.md#screen-rotation-and-resize).

## Facilitators (all variants: base, Px, Plain, Res)

Markup parity for **base** facilitators (Rotate / Mode / Qualifier / Screen):

| Android-style name | C# | Short | Normal |
|--------------------|-----|-------|--------|
| `sdpRotate`, `hdpRotate`, `wdpRotate` | ✅ | ✅ | ✅ |
| `sspRotate`, `hspRotate`, `wspRotate`, `semRotate`, `hemRotate`, `wemRotate` | ✅ | ✅ | ✅ |
| `sdpMode`, `sspMode`, … `wemMode` | ✅ | ✅ | ✅ |
| `sdpQualifier`, `sspQualifier`, … `wemQualifier` | ✅ | ✅ | ✅ |
| `sdpScreen`, `sspScreen`, … `wemScreen` | ✅ | ✅ | ✅ |
| `*Px()` physical pixels | ✅ | — | — |

Note: Android documents `hdpMode` / `hdpQualifier` / `hdpScreen` names; in MAUI only **SDP** has Mode/Qualifier/Screen on the DP axis. HDP/WDP expose **Rotate** only.

## Builders

- `DimenScaled` / `DimenSspScaled` priorities 1–4
- Sort: priority asc, qualifierValue desc

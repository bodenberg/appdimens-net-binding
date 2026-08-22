# Audit Report — Bodenberg.AppDimens.Maui.Sdps 3.6.0

Date: 2026-08-22
Scope: full audit of `appdimens-sdps-net` against the reference Android library
[appdimens-sdps v3.1.7](https://github.com/bodenberg/appdimens-sdps), logic-error fixes,
performance work, the new resize-independent (`i`) contract, and end-to-end testing on
desktop (net8/9/10), browser (Chrome 151 via CDP) and Android devices.

## 1. Findings and resolutions

### Bugs fixed

| ID | Severity | Finding | Fix |
|----|----------|---------|-----|
| B1 | High | `InverterEngine.ForHdpPw()` returned `PwToLh` (landscape, width-axis) — `hdpPw` never inverted in portrait | Returns `LhToPw` (portrait HEIGHT→WIDTH), matching Android `LH_TO_PW` |
| B2 | High | `InverterEngine.ForWdpPh()` returned `PhToLw` — `wdpPh` never inverted in portrait | Returns `LwToPh` (portrait WIDTH→HEIGHT), matching Android `LW_TO_PH` |
| B3 | High | `SetFontScale()` did not invalidate cached sp values — stale font sizes after system text-scale change | Font scale now participates in the cache key (`DimenCacheKey.FontScaleBits`) |
| B4 | Medium | Resize handling observed only display-level changes; window resizes (desktop/split-screen/foldables) were invisible to live values | `AppDimensSdpsWindow.Attach(Window)` + resolver window-bounds override merged into `RefreshMetricsFromDevice` |
| B5 | Low | `MmToPx` written as `mm*density*160/25.4/25.4*25.4` — numerically correct but obscure | Cleaned to `mm / 25.4 * density * 160`; regression tests added (exact inch/cm conversions) |

### Contract change (product requirement)

| ID | Change |
|----|--------|
| C1 | **Suffix `i` = resize-independent.** New baseline snapshot (`AppDimensResolver.CaptureBaseline`, captured at `Initialize`) backs all independent APIs: `Sdpi/Sdpia/Hdpi/Hdpia/Wdpi/Wdpia/Sspi/Sspia/Semi/Semia/Hemi/Wemi`. Live APIs continue to auto-adjust on any screen/window resize. Independent bucket manager + aspect-ratio factors are separate instances so frozen values cannot drift when live buckets re-select. |
| C2 | XAML surface for the new family: `Sdpi/Sdpia/Hdpi/Hdpia/Wdpi/Wdpia/Sspi/Sspia` markup extensions, short-markup tokens (`{sdpi:16}`, `{hdpi:48}`, `{wdpi:120}`, `{sspi:14}`, `{sdpia:16}`, …) and `Independent="True"` on Sdp/Ssp/Hdp/Wdp converters. |
| C3 | `UiModeType` gained foldable states (`FoldOpen/FoldClosed/FoldHalfOpened/FlipOpen/FlipClosed/FlipHalfOpened`) for Android v3.1.7 parity. |

### Performance

| ID | Improvement |
|----|-------------|
| P1 | `DimenScaled` builder no longer clones + sorts the rule list on every `Add`; ordering is computed once lazily at first resolve (`_sorted`). |
| P2 | Resolver subscribes to `IScreenMetricsProvider.Changed` — a single invalidation pipeline for every metrics source (display events, window watcher, custom providers, tests); removes duplicated refresh paths. |
| P3 | Bucket-mode resolution keeps pre-calculated table lookups on the hot path; continuous fallback unchanged. Cache keys are bit-packed structs with per-source partitioning (`AxisSource` live/baseline). |

## 2. Verification evidence

### Machine (desktop)

- `dotnet test` — **73/73 passing** on **net8.0, net9.0 and net10.0** (xUnit; includes 17 new tests:
  independent-freeze contract, baseline capture/re-capture, bucket-of-baseline, font-scale
  regression, inverter regressions B1/B2, physical units).
- Full solution builds with zero warnings on all TFMs; net11.0 is conditionally enabled when an SDK ≥ 11 is present.
- `dotnet pack` produces `Bodenberg.AppDimens.Maui.Sdps.3.6.0.nupkg`.

### Browser

- New sample `samples/AppDimens.Maui.BrowserDemo` (Blazor Server, engine-only references — no MAUI dependency).
- Real-browser test with **Google Chrome 151 headless driven over the DevTools Protocol**:
  - In-page self-test: **5/5 PASS** (sdpi == sdp at baseline · sdp adjusts · sdpi frozen · positive hdpi/wdpi · bucket tables loaded).
  - Window resized 800×600 → 1280×900 via `Emulation.setDeviceMetricsOverride`:
    live `sdp16` 25.60 → **48.00**, frozen `sdpi16` stayed **25.60**, `sdpia16` stayed **25.32**.
  - Verdict: **PASS** (screenshot: `/tmp/kilo/browser-demo-1280.png`).

### Devices (ADB)

- Physical device Xiaomi 2107113SG (Android 14, arm64): arm64 APK installed, launched,
  window watcher attached (`window bounds 392.7×872.7 dp applied`), baseline logged
  (`sw=393 sdp16=20.95 sdpi16=20.95`). MIUI blocks shell-driven `wm size`/rotation;
  screenshot captured (`/tmp/kilo/phone-home.png`).
- Emulator API 37 x86_64: full resize scenario executed with `wm size`:

  | Step | sw (dp) | sdp16 (live) | sdpi16 (frozen) |
  |------|---------|--------------|-----------------|
  | Baseline 1080×2400 | 411 | 21.94 | 21.94 |
  | `wm size 720x1600` | 274 | **14.63** ✅ adjusts down | **21.94** ✅ frozen |
  | `wm size reset` | 411 | 21.94 ✅ returns | 21.94 ✅ still frozen |
  | `wm size 1600x2560` | 610 | **32.51** ✅ adjusts up | **21.94** ✅ frozen |

  Screenshots: `device-resized.png`, `device-normal.png`, `device-large.png`.

## 3. Residual risks / notes

1. Playwright-bundled Chromium/Firefox fail to execute .NET 10 WASM on this workstation
   (even the default template hangs) — environment limitation, not library code; the
   browser test therefore runs the Blazor Server host in real Chrome via CDP, exercising
   the same engine modules plus real browser resize events.
2. MIUI restricts secure settings from adb on the physical phone; resize scenarios there
   are covered by the emulator run (same APK pipeline, same watcher code path — the
   physical device log already shows the watcher applying window bounds).
3. Android SDPS aliases `sdpia → sdpa`; MAUI intentionally gives the `i` suffix the
   stronger resize-independent semantics described above (documented divergence).

## 4. Version

All libraries in this folder ship as **3.6.0** (`Directory.Build.props`); sample app
display version bumped accordingly.

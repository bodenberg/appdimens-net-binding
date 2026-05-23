# AppDimens MAUI Sample (cross-platform)

Demonstration app with shared code on **Android**, **iOS**, **Mac Catalyst**, and **Windows**.

## Platforms

| Target | OS | Typical build |
|--------|-----|---------------|
| `net10.0-android` | Android 7+ (API 24+) | Linux, macOS, Windows |
| `net10.0-ios` | iPhone / iPad 15+ | **macOS** (Xcode) |
| `net10.0-maccatalyst` | macOS 15+ (Mac app) | **macOS** (Xcode) |
| `net10.0-windows10.0.19041.0` | Windows 10 1809+ | **Windows** |

On **Linux**, the `.csproj` includes Android only (iOS/Mac/Windows require Apple SDKs or WinUI). On macOS and Windows, the other targets are added automatically.

## Workloads

```bash
dotnet workload install maui-android          # Android (any OS)
dotnet workload install maui-ios              # macOS
dotnet workload install maui-maccatalyst      # macOS
# Windows: maui-windows workload ships with the SDK on Windows
```

## Build per platform

```bash
cd appdimens-sdps-net/samples/AppDimens.Maui.Sample

# Android (Linux/macOS/Windows) — JDK 21 required for SDK 36+
export JAVA_HOME=/usr/lib/jvm/java-21-openjdk
dotnet build -f net10.0-android

# iOS (macOS only)
dotnet build -f net10.0-ios

# Mac Catalyst (macOS only)
dotnet build -f net10.0-maccatalyst

# Windows (Windows only)
dotnet build -f net10.0-windows10.0.19041.0
```

## Run

```bash
# Android — device or emulator
dotnet build -t:Run -f net10.0-android

# iOS — simulator or device (macOS)
dotnet build -t:Run -f net10.0-ios

# Mac
dotnet build -t:Run -f net10.0-maccatalyst

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

## App content

| Tab | Content |
|-----|---------|
| Home | Screen metrics, `{sdp:16}` vs `{dimen:Sdp Value=16}` comparison, navigation |
| Dimensions | **Short** `{sdp:16}`, `{hdp:48}` · **Long** `{dimen:Sdp Value=16}` · **C#** `16.Sdp()` |
| Inverters | DP/SP axis inverters + AR (`{sdpPh:16}`, `{sdpLwA:16}`, `{semaPh:18}`…) — short, long, and C# |
| Advanced | `{scaled:…}`, Rotate/Mode/Qualifier/Screen — short, long, and C# |
| Benchmark | SDP/SSP/HDP/WDP/SDPA lookup timing on device |

### Short syntax (`AppDimens.Maui.Markup`)

Declare one xmlns prefix per extension (same CLR namespace):

```xml
xmlns:sdp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:ssp="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:sdpPh="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"
xmlns:scaled="clr-namespace:AppDimens.Maui.Markup;assembly=AppDimens.Maui.Markup"

<VerticalStackLayout Padding="{sdp:20}" Spacing="{sdp:8}">
    <Label FontSize="{ssp:16}" />
    <BoxView HeightRequest="{sdp:48}" CornerRadius="{sdp:6}" />
    <Label FontSize="{sdpPh:16}" />
</VerticalStackLayout>
```

| MAUI property | Short | Long (`dimen:`) |
|---------------|-------|-----------------|
| Padding, Margin | `{sdp:20}` | `{dimen:Sdp Value=20}` |
| FontSize | `{ssp:16}` | `{dimen:Ssp Value=16}` |
| PH inverter | `{sdpPh:16}` | `{dimen:Sdp Value=16, Inverter=SwToPh}` |
| SDPA+LW inverter | `{sdpLwA:16}` | `{dimen:Sdpa Value=16, Inverter=SwToLw}` |
| Responsive | `{scaled:14, Tablet=18, Landscape=16}` | `{dimen:Scaled Value=14, Tablet=18, …}` |
| Rotate | `{sdpRotate:30, Rotation=45, Orientation=Landscape}` | `{dimen:SdpRotate Base=30, …}` |
| Qualifier | `{sdpQualifier:16, Qualified=20, QualifierType=SmallWidth, Threshold=600}` | `{dimen:SdpQualifier Base=16, …}` |
| Mode | `{sdpMode:16, Mode=24, UiMode=Desk}` | `{dimen:SdpMode Base=16, Mode=24, UiMode=Desk}` |
| Screen | `{sdpScreen:16, Screen=20, UiMode=Television, QualifierType=SmallWidth, Threshold=600}` | `{dimen:SdpScreen Base=16, …}` |

Each short extension accepts **any integer index** (`{sdp:20}`, `{sdpLwA:24}`, …). Sample values are for visual comparison only.

`Padding`/`Margin` receive a uniform `Thickness` automatically in **both** syntaxes (requires `Inflator="Runtime"` in `.csproj` for `IProvideValueTarget`).

**Without XAML markup:** physical pixels (`16.SdpPx()`), binding converters, chained `Responsive.Value(…).Screen(…)` beyond `{scaled:…}` shortcuts.

### Long syntax (`AppDimens.Maui.Xaml`)

```xml
xmlns:dimen="clr-namespace:AppDimens.Maui.Xaml;assembly=AppDimens.Maui.Xaml"

<VerticalStackLayout Padding="{dimen:Sdp Value=20}"
                   Spacing="{dimen:Sdp Value=8}"
                   Margin="{dimen:Sdp Value=12}">
    <Label FontSize="{dimen:Ssp Value=16}" />
    <BoxView HeightRequest="{dimen:Hdp Value=48}" CornerRadius="{dimen:Sdp Value=6}" />
</VerticalStackLayout>
```

Sample project references: `AppDimens.Maui.Markup` (short) + `AppDimens.Maui.Xaml` (long).

Global styles in `Resources/Styles/Styles.xaml` use literals (markup in `Style` setters does not compile in XamlC).

### Screen rotation

| Step | Required? | Location |
|------|-----------|----------|
| `AppDimensSdps.Initialize()` / `UseAppDimensSdps()` | **Yes** | `MauiProgram.cs` — metrics + `MainDisplayInfoChanged` |
| `ConfigurationChanges` (Android) | **Recommended** | `Platforms/Android/MainActivity.cs` — activity is not recreated on rotation |
| `OnConfigurationChanged` → `RefreshMetricsFromDevice()` | **Recommended (Android)** | Same file — refreshes metrics and cache |
| `DimensSampleRefresh.WhenMetricsChange` | **Optional (live UI)** | Tabs with dynamic C# values (see below) |
| Manifest / extra AppDimens file | **No** | — |

**Library (automatic):** on rotate or resize, the resolver updates `widthDp` / `heightDp` / `smallestDp` / orientation, invalidates the dimension cache, and new `16.Sdp()` calls use current metrics.

**XAML markup** (`{sdp:16}`, inverters, `{scaled:…}`): evaluated **at page load** — `Padding`, `FontSize`, etc. keep previous pixels until the page is rebuilt or updated in code.

**C# in the sample:** labels and cards built in code-behind refresh on rotation via `DimensSampleRefresh` + `Metrics.Changed`:

| Tab | Refresh on rotation |
|-----|---------------------|
| Home | Screen metrics (`ScreenMetricsFormatter`) |
| Dimensions | Short/long/C# pixel values |
| Inverters | Inverters + pixel comparison |
| Advanced | Rotate, Mode, Qualifier, Screen, Responsive |
| Benchmark | Completed cards (fixed timings; last value re-resolved) |

Example (used on all pages above):

```csharp
public HomePage()
{
    InitializeComponent();
    DimensSampleRefresh.WhenMetricsChange(this, RefreshMetrics);
}
```

Android (`MainActivity.cs` no sample):

```csharp
public override void OnConfigurationChanged(Configuration newConfig)
{
    base.OnConfigurationChanged(newConfig);
    AppDimensResolver.Instance.RefreshMetricsFromDevice();
}
```

Full documentation: [library README — Screen rotation and resize](../../README.md#screen-rotation-and-resize).

## Android: startup crash (`lib/x86_64` / `.__override__`)

If logcat shows `No assemblies found in .../__override__/x86_64` and `ALL entries in APK named lib/x86_64/ MUST be STORED`, the cause is packaging (Fast Deployment), not AppDimens. The `.csproj` disables Fast Deployment, embeds assemblies in the APK, and stores only `.so` uncompressed (do not add `.dll` to `AndroidStoreUncompressedFileExtensions` — that inflates the APK to 200 MB+).

**Recovery on device/emulator:**

```bash
adb uninstall com.bodenberg.appdimens.sample
cd samples/AppDimens.Maui.Sample
dotnet clean
rm -rf bin obj
dotnet build -t:Run -f net10.0-android
```

## APK size

The **~250 MB** Debug APK came from three factors:

1. **Two ABIs** in one APK (`arm64-v8a` + `x86_64`, ~125 MB each).
2. **`EmbedAssembliesIntoApk`** (required to avoid Fast Deployment crash).
3. **`AndroidStoreUncompressedFileExtensions` with `.dll`** — forced all assemblies uncompressed (fixed: `.so` only).

| Build | Single ABI (`android-x64` on Linux) | Notes |
|-------|-----------------------------------|-------|
| **Release** | **~30 MB** | AOT + embedded assemblies; use for real size testing |
| **Debug** | **~130 MB** | Embedded assemblies (no Fast Deployment), debug symbols |

On **Linux**, the `.csproj` sets `RuntimeIdentifiers=android-x64` and `AndroidCreatePackagePerAbi=true`. Debug and Release **embed** assemblies (`EmbedAssembliesIntoApk=true`) so the app launches on the emulator without IDE incremental deploy.

```bash
# Development with breakpoints (~130 MB, self-contained emulator install)
dotnet build -t:Run -f net10.0-android

# Smaller APK (~30 MB), production-like behavior
dotnet build -t:Run -f net10.0-android -c Release
```

Physical ARM device: `dotnet build -t:Run -f net10.0-android -r android-arm64`.

## `Platforms/` layout

```
Platforms/
├── Android/     MainActivity, MainApplication, AndroidManifest, colors
├── iOS/         AppDelegate, Program, Info.plist, PrivacyInfo
├── MacCatalyst/ AppDelegate, Program, Info.plist, Entitlements
└── Windows/     App.xaml (WinUI), Package.appxmanifest, app.manifest
```

UI code (`Views/`, `AppShell`, `MauiProgram`) is **shared** across all platforms.

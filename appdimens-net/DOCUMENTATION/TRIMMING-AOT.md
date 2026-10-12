# Trimming & AOT readiness

## Library

All `AppDimens.Net.*` libraries are plain C# with no reflection on hot paths:

* No `Activator.CreateInstance`, no dynamic dispatch, no `Type.GetType`.
* Caches are keyed by value tuples/longs — trimmer-safe.
* `[DynamicallyAccessedMembers]` not required anywhere.

The libraries carry `<IsAotCompatible>true</IsAotCompatible>`-compatible code
patterns and can be consumed by AOT/trimmed MAUI heads.

## MAUI heads (Sample / BenchLab)

```xml
<PublishTrimmed>true</PublishTrimmed>
<RunAOTCompilation>true</RunAOTCompilation>   <!-- Android: LLVM optional -->
<TrimMode>full</TrimMode>
```

Notes for full trimming:

1. XAML is not used by the sample apps (pure C# UI) — nothing to preserve.
2. If you add XAML pages, keep `XamlC.CompiledBindings` default; the trimmer
   handles compiled bindings without extra steps.
3. `AppDimensAmbient.Require()` failures surface as
   `InvalidOperationException` at first use, never as silent AOT stubs.

## WebAssembly (`samples/AppDimens.WebDemo`)

Builds and runs under .NET WASM out of the box; the self-test suite executes in
browser and reports 21/21 (see `DOCUMENTATION/PARITY.md`).

## Desktop adapters (WPF / Windows Forms / WinUI 3)

`AppDimens.Net.Wpf`, `.WinForms` and `.WinUI` add no reflection on the hot path. The WinUI markup extensions look up
the target `XxxProperty` dependency-property field once per XAML assignment (public static fields of WinUI controls,
never trimmed; the call is annotated with `UnconditionalSuppressMessage`). WPF itself is not trim-compatible, so WPF
apps are normally published untrimmed.

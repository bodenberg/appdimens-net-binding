# Maintenance Plan

| Cadence | Task |
|---------|------|
| Each PR | CI: test + pack verify + vulnerable packages |
| Monthly | Review Xamarin.Android dependency pins in bindings |
| Quarterly | Sync AAR from Maven (`sync-aar-from-maven.sh`) |
| Per release | Regenerate dimens if Android generator constants change |
| Yearly | Multi-TFM deprecation review (net8 support) |

## Ownership

- MAUI native: `appdimens-sdps-net/`
- Bindings: respective `*-net-binding/` folders
- Parity: keep `docs/PARITY_MATRIX.md` aligned with Android v3.1.5+

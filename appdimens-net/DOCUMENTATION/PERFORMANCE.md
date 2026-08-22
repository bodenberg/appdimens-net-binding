# Performance — AppDimens .NET

Measured on-device by `benchlab/AppDimens.BenchLab` (Android emulator x86_64,
Mono JIT, warmup 10k, measured 200k per row, 8 rows, total ≈ 271 ms).

## Results (emulator, Release)

| Row | ns/op | ops/s | alloc/op |
|---|---:|---:|---:|
| baseline raw multiply (dp) | 3.2 | 312.9 M | 0 B |
| sdp fast lane (`ResolveSdpDp`) | 8.8 | 113.3 M | 0 B |
| sdpa fast lane (aspect-ratio) | 8.2 | 121.4 M | 0 B |
| hdp fast lane | 10.2 | 98.2 M | 0 B |
| full cached path (`ToDynamicScaledDp`) | 69.4 | 14.4 M | 0 B |
| uncached full formula | 24.4 | 40.9 M | 0 B |
| legacy v1 XML-grid lookup | 34.6 | 28.9 M | 0 B |
| fluid cached | 112.1 | 8.9 M | 0 B |

## Reading the numbers

* The **fast lanes** are ~2.7× slower than a raw multiply but ~8× faster than
  the generic cached path — and allocate **zero bytes** on the hit path.
* The **full cached path** pays for key building + snapshot lookup; it is still
  allocation-free and comfortably fast for UI code paths.
* The **legacy v1 approach** (pre-generated XML dimension tables, dictionary
  lookup) is ~4× slower than the fast lane while covering only integer values.
* All rows are chunked and run off the UI thread with progress callbacks; a
  reentrancy guard prevents overlapping runs (KMP BenchlabController parity).

## Design notes

* Cache is snapshot-partitioned: each window configuration owns its entries;
  resize invalidates by partition swap, not by clearing dictionaries under lock.
* Fast metrics (`Scale`, aspect multipliers) are precomputed once per snapshot in
  `DimenMetrics`; lanes multiply in kernel order to stay bitwise-equal to the
  full path.
* Diagnostics counters are opt-in (`DimenCache.DiagnosticsEnabled`) so the hit
  path stays zero-overhead by default.

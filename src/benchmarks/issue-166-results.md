# Issue 166: shared semantic models

The fix is on `perf/share-semantic-model-per-tree`, based on `b0c70a0`.
The comparison worktree is `/tmp/rxb-before`.

The generator shares one semantic model per source file in its compilation copy.
A `ConditionalWeakTable` ties the cache to that compilation's lifetime.
A `ConcurrentDictionary` lets parallel readers share the same model.
Generated output and public APIs stay the same.

## Real application

The [issue](https://github.com/reactiveui/ReactiveUI.Binding.SourceGenerators/issues/166) names
v2rayN commit `09a84ded3dfbf762adddde9375603c0a68d546fc`.
The local reproduction is `/tmp/v2rayN`.
Each measurement follows a comment edit in `MainWindow.axaml.cs`.
The compiler server is warm. Each result is the median of three builds.

| Measurement | Baseline | Fixed |
| --- | ---: | ---: |
| Generator time | 10.968 s | 0.441 s |
| Process wall time | 17.445 s | 5.806 s |

The local packages are `9.0.1-base166` and `9.0.1-perf166`.
They use the same repository base.
Windows example checks ran during these builds, so absolute timings include shared-machine noise.
The saved earlier runs also show the gain.

All 60 saved generated files match byte for byte.
Binding's eight files total 6,866,986 bytes and 90,331 lines in each build.
This fix removes repeated semantic binding. It does not reduce emitted code size.

## Synthetic benchmark

Four views bind 10, 50 or 100 controls each in one activation lambda.
The XAML case also uses a `[Reactive]` field.
The control case declares fields directly.
Setup checks that generated code compiles.
Both measured methods reject output sizes that differ from setup.

Fresh runs use BenchmarkDotNet on .NET 10 and 11.
They pin processes to physical cores 0 through 6.
Each case has three warmups and eight measured iterations.
Profilers are disabled for timing. GC uses concurrent workstation mode.
The CPU governor is `powersave`. Higher process priority is unavailable.
Small timing differences need caution on this shared machine.

Cold generation with XAML, mean ± BenchmarkDotNet error:

| Runtime | Bindings per view | Baseline | Fixed | Baseline allocation | Fixed allocation |
| --- | ---: | ---: | ---: | ---: | ---: |
| .NET 10 | 10 | 128.35 ± 4.797 ms | 20.08 ± 0.354 ms | 33.38 MB | 5.22 MB |
| .NET 10 | 50 | 2,764.30 ± 74.607 ms | 101.85 ± 6.262 ms | 826.55 MB | 28.77 MB |
| .NET 10 | 100 | 11,950.64 ± 240.571 ms | 228.37 ± 18.160 ms | 3,703.48 MB | 61.20 MB |
| .NET 11 | 10 | 121.11 ± 4.091 ms | 20.80 ± 1.132 ms | 33.36 MB | 5.22 MB |
| .NET 11 | 50 | 3,065.43 ± 558.707 ms | 96.40 ± 4.076 ms | 826.23 MB | 28.76 MB |
| .NET 11 | 100 | 12,793.31 ± 494.441 ms | 203.66 ± 8.244 ms | 3,702.24 MB | 61.18 MB |

Direct-compilation allocation differs by at most 0.02 MB in these runs.
Its timing differences are noisy. This change targets the compilation-copy path.

The saved comment-edit measurements on .NET 10 also show the gain.
At 100 bindings per view, edit generation falls from 11,192 ± 231 ms to 209 ± 78 ms.
Allocations fall from 3,684 MB to 42 MB.

## Allocation profile

The .NET 10 EventPipe traces include CPU samples and verbose GC events.
In the baseline, binding extraction contains 67.6% of samples with a generator frame.
Command extraction contains 31.0%.
Compilation-copy construction contains about 0.1%. XAML reading and resolution each contain less than 0.1%.
These are inclusive sample shares, not wall-clock percentages.

Most baseline allocation samples are Roslyn binder and symbol objects beneath those extractors.
The fixed trace has 2,393 allocation ticks attributed to generator frames.
The trace cannot resolve the cache's small setup allocations.
The benchmark allocation columns measure total bytes per generation.

All 12 fixed cold-generation and comment-edit cases were also exercised with tracing enabled.

## Verification

- Release solution-filter build: no warnings or errors.
- Full Linux suite: 13,126 passed; 138 skipped; no failures.
- `CallSiteContext.cs`: 100% line and branch coverage across both generator builds and runtimes.
- Changed-file formatting and analyzer check passed.
- All Linux documentation and platform examples passed.
- Native AOT publish and execution passed.
- Windows property, platform and WPF/WinForms threading examples passed.

The Windows mirror lacks git metadata. Version stamping was skipped for its example runs.
Those builds emit source-link metadata warnings.
No analyzer suppression was added.

Logs and measurement artifacts are under `/tmp/issue166-*`.

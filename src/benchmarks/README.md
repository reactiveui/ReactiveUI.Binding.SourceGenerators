# Benchmarks

This folder measures two different things. One is what a consumer's application pays at run time when a
binding fires. The other is what a consumer's build pays while the generator runs.

Numbers are not published here. Hardware and runtime versions move, and a figure written down goes stale
without anyone noticing. Run the suite and read your own.

## The projects

| Project | Measures |
|---------|----------|
| `ReactiveUI.Binding.Benchmarks` | The generated observation and binding path. |
| `ReactiveUI.Binding.Benchmarks.ReactiveUI` | The same scenarios on ReactiveUI's expression-tree engine, as the baseline. |
| `ReactiveUI.Binding.Generator.Benchmarks` | A whole generation pass over a corpus of consumer code. |
| `ReactiveUI.Binding.Generator.Benchmarks.Roslyn413` | The same generation pass against Roslyn 4.13. |

The baseline lives in its own project because it needs ReactiveUI's own binding surface. Keeping the two
apart stops one engine's imports reaching the other's call sites.

## What the runtime benchmarks cover

Each case creates one observation or binding and then drives a thousand property changes through it. The
cost of creating the subscription is measured separately, by the `First...` cases.

| Benchmark | Cases |
|-----------|-------|
| `WhenChangedBenchmark` | `SingleProperty`, `DeepChain`, `TwoProperties`, `FirstObservation` |
| `BindOneWayBenchmark` | `Standard`, `WithScheduler`, `FirstBinding`, `SetupTeardown` |
| `BindTwoWayBenchmark` | `Standard`, `WithScheduler`, `Bidirectional` |
| `BindBenchmark` | `Standard`, `Bidirectional`, `WithObservedChanges` |
| `WhenAnyDynamicBenchmark` | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation`, `SingleChainGenerated` |
| `RxUiDynamicChainBaseline` | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation` |

`WhenAnyDynamicBenchmark` carries `SingleChainGenerated` so the two halves of the same scenario sit in one
table. A chain named at run time is walked by reflection; the same chain written as a lambda is resolved at
compile time.

Each class declares a job per runtime: .NET 8, 10 and 11, and NativeAOT 10 and 11 where the code can run
ahead of time. A class that walks a chain by reflection declares no NativeAOT job, because it cannot run
there.

The .NET Framework 4.6.2 job is opt-in. Set `BenchNetFx` to add it, on Windows only, since no other host can
launch it:

```sh
dotnet run -c Release -f net10.0 --property:BenchNetFx=true -- --filter '*'
```

That leg and the EventPipe profiler are mutually exclusive. The profiler refuses any job below .NET Core 3.0
and its validator stops the whole run rather than the single job, so `BenchNetFx` drops it. A default run
keeps the profiler and the allocation traces; a `BenchNetFx` run trades them for the older runtime, and
`MemoryDiagnoser` still reports the allocation column.

## What the generation benchmark covers

`GenerationBenchmarks.Generate` runs a cold pass: syntax scan, extraction and emission. Two parameters
vary.

| Parameter | Values | Meaning |
|-----------|--------|---------|
| `Pairs` | 1, 16, 64 | How many view-model and view pairs the corpus holds. |
| `Intercept` | false, true | Whether the build claims each call site outright or offers an overload that competes for them all. |

A fresh generator driver is built for every iteration. A reused driver would serve the next iteration from
its incremental caches, which measures the cache rather than the pass a consumer's build actually pays for.
The corpus is built once per parameter set, because loading a framework's worth of metadata references
costs far more than the pass under measurement.

The benchmark runs under `MemoryDiagnoser` and the `GcVerbose` EventPipe profiler. The allocation column is
the A/B number, and the trace names the frame that allocated.

## What is not measured yet

Five operators are benchmarked: `WhenChanged`, `WhenAnyDynamic`, `BindOneWay`, `BindTwoWay` and `Bind`.
`WhenAnyValue` and `OneWayBind` are measured only on the ReactiveUI side, so those two rows have no
counterpart. `WhenChanging`, `WhenAny`, `WhenAnyObservable`, `BindTo`, `BindCommand`, `BindInteraction` and
`InvokeCommand` have no benchmark at all, and neither has any `Unsafe` overload.

An `Unsafe` overload resolves its expression by reflection, so it can carry the managed jobs but no NativeAOT
job, the same way the dynamic-chain classes do.

## Running them

Each project hands its command line to the BenchmarkDotNet switcher, so `--filter` selects cases.

```sh
cd src/benchmarks/ReactiveUI.Binding.Benchmarks
dotnet run -c Release -f net10.0 -- --filter '*'

cd ../ReactiveUI.Binding.Benchmarks.ReactiveUI
dotnet run -c Release -f net10.0 -- --filter '*'

cd ../ReactiveUI.Binding.Generator.Benchmarks
dotnet run -c Release -f net10.0 -- --filter '*'
```

Release configuration is required. A Debug build measures the absence of the optimiser.

Results land in `BenchmarkDotNet.Artifacts/results`, which is ignored by git.

## Reading the results

Three things are worth knowing before comparing columns.

The generated path removes reflection and expression compilation, so it wins on both time and allocation
against the baseline. Compare `WhenChangedBenchmark` against `ReactiveUIObservationBenchmark`, and
`BindOneWayBenchmark` and `BindBenchmark` against `ReactiveUIBindingBenchmark`.

A chain named at run time is the one place the two engines are level. Both walk the chain by reflection and
allocate the same objects doing it, so `WhenAnyDynamicBenchmark` and `RxUiDynamicChainBaseline` land on top
of each other. The gain there comes from writing the chain as a lambda instead, which
`SingleChainGenerated` measures.

The expression-tree engine cannot run under NativeAOT at all, because it compiles expressions and reflects
at run time. Only the generated path has a NativeAOT column to report.

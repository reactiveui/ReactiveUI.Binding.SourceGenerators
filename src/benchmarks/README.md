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

| Benchmark | Operator | Cases |
|-----------|----------|-------|
| `WhenChangedBenchmark` | `WhenChanged` | `SingleProperty`, `DeepChain`, `TwoProperties`, `FirstObservation` |
| `WhenChangingBenchmark` | `WhenChanging` | `SingleProperty`, `TwoProperties`, `FirstObservation` |
| `WhenAnyValueBenchmark` | `WhenAnyValue` | `SingleProperty`, `DeepChain`, `TwoProperties`, `FirstObservation` |
| `WhenAnyBenchmark` | `WhenAny` | `SingleProperty`, `TwoProperties`, `FirstObservation` |
| `WhenAnyObservableBenchmark` | `WhenAnyObservable` | `SingleStream`, `TwoStreams`, `FirstObservation` |
| `WhenAnyDynamicBenchmark` | `WhenAnyDynamic` | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation`, `SingleChainGenerated` |
| `BindOneWayBenchmark` | `BindOneWay` | `Standard`, `WithScheduler`, `FirstBinding`, `SetupTeardown` |
| `BindTwoWayBenchmark` | `BindTwoWay` | `Standard`, `WithScheduler`, `Bidirectional` |
| `BindBenchmark` | `Bind` | `Standard`, `Bidirectional`, `WithObservedChanges` |
| `OneWayBindBenchmark` | `OneWayBind` | `Standard`, `FirstBinding` |
| `BindToBenchmark` | `BindTo` | `Standard`, `FirstBinding` |
| `InvokeCommandBenchmark` | `InvokeCommand` | `Standard`, `FirstInvocation` |
| `UnsafeFallbackBenchmark` | the `Unsafe` overloads | `WhenChangedUnsafe`, `WhenChangedUnsafe deep chain`, `WhenAnyValueUnsafe`, `BindOneWayUnsafe`, `BindUnsafe` |
| `RxUiDynamicChainBaseline` | ReactiveUI's dynamic chain | `SingleChain`, `TwoChains`, `DeepChain`, `FirstObservation` |

`WhenAnyValueBenchmark` and `OneWayBindBenchmark` exist so the two operators the ReactiveUI baseline measures
have a counterpart here. Read each against `ReactiveUIObservationBenchmark` and `ReactiveUIBindingBenchmark`.

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

## What is not measured

`BindCommand` and `BindInteraction` have no benchmark. Both need something registered before they do any
work: `BindCommand` needs an `ICreatesCommandBinding` that reaches the control, and `BindInteraction` needs a
handler. A benchmark would be measuring that fixture rather than the library, so the number would not mean
what it appeared to.

Everything else in the observation and binding surface is covered, including the `Unsafe` overloads.

`UnsafeFallbackBenchmark` declares no NativeAOT job. Those overloads carry `RequiresUnreferencedCode` because
they walk the path at run time, so an ahead-of-time publish cannot be relied on to keep the members they
reach. That class is the measurement of what the fallback costs against the generated operator of the same
name.

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

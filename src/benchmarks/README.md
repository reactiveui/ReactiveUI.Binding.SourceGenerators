# Benchmarks

This folder measures two different things. One is what a consumer's application pays at run time when a
binding fires. The other is what a consumer's build pays while the generator runs.

## What it costs

Measured by the `Benchmarks` workflow on a GitHub-hosted Windows runner: two physical cores under Hyper-V,
.NET 10.0.12. Each case drives a thousand property changes through one subscription or binding.

Allocation is deterministic and compares directly. The timings carry real spread on a runner that size - the
observation cases move by twenty to thirty per cent between iterations - so read them as the shape of the
difference rather than as a score, and run the suite on your own hardware for a number you can hold someone
to.

| Observation | ReactiveUI's engine | `WhenChanged` | `WhenAnyValue` |
|-------------|--------------------:|--------------:|---------------:|
| One property | 145.6 us / 105.5 KB | 81.7 us / 65.5 KB | 79.5 us / 65.5 KB |
| A deep chain | 120.9 us / 106.2 KB | 108.5 us / 66.2 KB | 107.6 us / 66.2 KB |
| Two properties | 340.5 us / 195.6 KB | 182.4 us / 90.5 KB | 142.3 us / 90.5 KB |
| Creating the subscription | 9.8 us / 1.5 KB | 9.9 us / 1.4 KB | 9.0 us / 1.4 KB |

| Binding | ReactiveUI's engine | Generated |
|---------|--------------------:|----------:|
| `OneWayBind` | 287.8 us / 130.4 KB | 114.5 us / 88.5 KB |
| `Bind` | 686.5 us / 932.3 KB | 53.6 us / 42.2 KB |
| Creating a one-way binding | 22.5 us / 5.4 KB | 11.3 us / 2.6 KB |

`BindOneWay` and `BindTwoWay` have no counterpart in the other engine to read against. They cost
93.1 us / 65.4 KB and 105.9 us / 87.8 KB, and naming a scheduler lowers both rather than raising them
(71.1 us and 90.9 us), because the write is then queued instead of applied inline.

Published ahead of time, the generated path measures within a few per cent of the same code on the JIT:
`WhenAnyValue` 83.6 us against 79.5 us, `BindOneWay` 93.3 us against 93.1 us, `Bind` 53.8 us against
53.6 us. The expression-tree engine has no figure here because it cannot run under NativeAOT at all.

The `Unsafe` overloads are the price of an expression the generator could not read: `WhenChanged` 146.5 us /
103.1 KB against 81.7 us / 65.5 KB generated, `BindOneWay` 223.2 us / 221.4 KB against 93.1 us / 65.4 KB,
`Bind` 405.5 us / 402.7 KB against 53.6 us / 42.2 KB.

A generation pass costs 3.6 ms and 1.9 MB for one view-model and view pair, 52.3 ms and 29.5 MB for sixteen,
and 213.6 ms and 118.1 MB for sixty-four. Whether the build claims call sites outright or offers a competing
overload makes no measurable difference.

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

On Windows, add `--artifacts-path C:\a`. BenchmarkDotNet generates a project per job beside the built
assembly, and the NativeAOT build writes below that again; from a deep checkout the linker's module
definition file lands past 260 characters, and every NativeAOT job then reports `NA` instead of a
measurement.

Results land in `BenchmarkDotNet.Artifacts/results`, which is ignored by git.

## Reading the results

Three things are worth knowing before comparing columns.

The generated path removes reflection and expression compilation, so it wins on both time and allocation
against the baseline. Compare `WhenChangedBenchmark` against `ReactiveUIObservationBenchmark`, and
`BindOneWayBenchmark` and `BindBenchmark` against `ReactiveUIBindingBenchmark`.

A chain named at run time is the one place this library does not win. Both engines walk it by reflection and
allocate the same objects doing it - 105.1 KB against 105.0 KB - and ReactiveUI is marginally the quicker of
the two, at 118.0 us against 128.7 us. The gain there comes from writing the chain as a lambda instead, which
`SingleChainGenerated` measures at 81.1 us / 65.5 KB.

The expression-tree engine cannot run under NativeAOT at all, because it compiles expressions and reflects
at run time. Only the generated path has a NativeAOT column to report.

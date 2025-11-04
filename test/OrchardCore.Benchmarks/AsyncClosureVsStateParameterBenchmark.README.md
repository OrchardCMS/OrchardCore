# Async Closure vs State Parameter Benchmark

## Overview

This benchmark compares the performance and memory allocation characteristics of different approaches to passing state to async delegates, specifically:

1. **Closure-based approach** - Captures variables from the outer scope
2. **Single state parameter** - Passes one parameter
3. **Two state parameters** - Passes two separate parameters  
4. **Value tuple as state** - Passes multiple values as a single value tuple parameter
5. **Value tuple with deconstruction** - Same as above but deconstructs immediately

## Purpose

This benchmark was created to validate the design decisions in `ShellScope.cs`, particularly around the `UsingAsync` pattern that now uses value tuples to avoid closure allocations.

## Benchmark Scenarios

### Simple Scenarios
- **ExecuteWithClosure** (Baseline) - Simple closure capturing two variables
- **ExecuteWithSingleStateParameter** - Single string parameter
- **ExecuteWithTwoStateParameters** - Two separate parameters
- **ExecuteWithValueTupleState** - Value tuple with Item1/Item2 access
- **ExecuteWithValueTupleStateDeconstruction** - Value tuple with deconstruction

### Complex Scenarios (Simulating ShellScope patterns)
- **ComplexScenarioWithClosure** - Multiple awaits with try/finally blocks using closure
- **ComplexScenarioWithValueTuple** - Same pattern with value tuple
- **ComplexScenarioWithTwoStateParameters** - Same pattern with two parameters

### Nested Scenarios (Simulating deferred tasks)
- **NestedAsyncWithClosure** - Nested async calls with closure
- **NestedAsyncWithValueTuple** - Nested async calls with value tuple

### Reference Type Scenarios
- **ReferenceTypeWithClosure** - Closure capturing a reference type
- **ReferenceTypeWithStateParameter** - Reference type as parameter

## Running the Benchmark

From the solution root:

```bash
cd test/OrchardCore.Benchmarks
dotnet run -c Release
```

To run only this benchmark:

```bash
dotnet run -c Release --filter *AsyncClosureVsStateParameterBenchmark* --framework net9.0
```

## Expected Results

Based on .NET 9 async machinery:

1. **Memory Allocations**: All async methods allocate for the state machine, so the differences are smaller than you might expect.

2. **Closure Cost**: Closures create an additional display class allocation plus the delegate allocation.

3. **Value Tuple Cost**: Value tuples are structs, but they still get captured in the async state machine's heap allocation.

4. **Trade-offs**: 
   - Closures: More allocations but simpler code
   - State parameters: Fewer allocations but more complex call sites
   - Value tuples: Good middle ground - fewer allocations than closures, cleaner than multiple parameters

## Key Insights

The benchmark demonstrates that:
- In async methods, the allocation difference between closures and value tuples is **smaller** than in synchronous code
- The async state machine captures all local variables regardless of approach
- Value tuples still provide **some benefit** by avoiding closure display class allocation
- The pattern is more about **consistency** and **marginal optimization** than dramatic performance gains

## Related Code

This benchmark is based on patterns used in:
- `src/OrchardCore/OrchardCore.Abstractions/Shell/Scope/ShellScope.cs`
- Particularly the `UsingAsync` and `UsingAsyncCore` methods

# Quick Start: ShellScope Performance Benchmarks

## TL;DR - Run This

```bash
cd test/OrchardCore.Benchmarks
dotnet run -c Release --filter "*ShellScope*"
```

## What You'll See

The benchmarks will compare the **optimized ShellScope** (current branch) against patterns that simulate the **original implementation** (main branch).

### Expected Output

```
BenchmarkDotNet v0.XX.XX
...

|                          Method |     Mean |   Allocated | Ratio |
|-------------------------------- |---------:|------------:|------:|
| SimpleRequest_OriginalPattern   | 1.234 μs |       360 B |  1.00 | <- Baseline
| SimpleRequest_OptimizedPattern  | 1.043 μs |        80 B |  0.84 | <- 16% faster, 78% less memory ✅
```

## Key Results to Look For

### 🎯 Primary Goal: Memory Reduction

Look for **70-90% reduction** in the **Allocated** column:

```
BeforeDispose_OriginalWithList    |  240 B  |  1.00
BeforeDispose_OptimizedWithIL     |   32 B  |  0.13  <- 87% less memory ✅
```

### 🚀 Secondary Goal: Speed Improvement

Look for **10-25% reduction** in the **Mean** time:

```
CompletePattern_OriginalPattern   | 2.456 μs |  1.00
CompletePattern_OptimizedPattern  | 1.847 μs |  0.75  <- 25% faster ✅
```

### 📊 Best Results

The biggest improvements will be in:

1. **`BeforeDispose_*` benchmarks** - Should show ~85-90% memory reduction
2. **`CompletePattern_*` benchmarks** - Should show ~70-80% memory reduction
3. **`HttpRequestPattern_*` benchmarks** - Realistic scenario, ~70-75% memory reduction

## What Each Benchmark Tests

### ShellScopeOptimizationBenchmark.cs

| Benchmark | What It Tests | Expected Improvement |
|-----------|---------------|---------------------|
| `UsingAsync_*` | State parameter vs closure | ~70% less memory |
| `BeforeDispose_*` | InlineList vs List | ~85% less memory |
| `DeferredTask_*` | Deferred task pattern | ~70% less memory |
| `CompletePattern_*` | Full ShellScope lifecycle | ~75% less memory |
| `HttpRequestPattern_*` | Realistic HTTP request | ~70% less memory |

### InlineListBenchmark.cs

| Benchmark | What It Tests | Expected Improvement |
|-----------|---------------|---------------------|
| `*_Add_SmallSize` | Adding 3 items | ~95% less memory |
| `*_Add_MediumSize` | Adding 8 items | ~90% less memory |
| `*_ShellScopeCallbackPattern` | Real usage | ~85% less memory |

### ShellScopeRealisticScenarioBenchmark.cs

| Benchmark | What It Tests | Expected Improvement |
|-----------|---------------|---------------------|
| `SimpleRequest_*` | Basic request | ~75% less memory |
| `RequestWithCallbacks_*` | With cleanup | ~80% less memory |
| `RequestWithDeferredTask_*` | Background work | ~75% less memory |
| `MultiTenantPattern_*` | Concurrent tenants | ~70% less memory |
| `ComplexRequest_*` | Full-featured | ~75% less memory |

## Quick Interpretation Guide

### ✅ Good Results

```
Allocated: 360 B -> 80 B  (78% reduction) ✅
Mean:      1.2 μs -> 1.0 μs (16% faster) ✅
```

### ⚠️ Investigate These

```
Allocated: 360 B -> 300 B (Only 17% reduction) ⚠️
Mean:      1.2 μs -> 1.4 μs (17% slower) ⚠️
```

### 🔴 Problems

```
Allocated: 360 B -> 500 B (39% MORE memory!) 🔴
Mean:      1.2 μs -> 2.0 μs (67% slower) 🔴
```

## Common Issues

### "No benchmarks found"

```bash
# Make sure you're in the right directory
cd test/OrchardCore.Benchmarks

# Make sure the project builds
dotnet build -c Release
```

### "Results seem wrong"

```bash
# Close other applications
# Run again - BenchmarkDotNet handles warmup automatically

# Or increase iterations for more accuracy
dotnet run -c Release --filter "*ShellScope*" -- --iterationCount 100
```

## Comparing Branches

### Option 1: Side-by-Side Comparison

```bash
# Terminal 1: Main branch
git checkout main
cd test/OrchardCore.Benchmarks
dotnet run -c Release --filter "*SimpleRequest*"

# Terminal 2: Optimization branch
git checkout gvkries/shellscope-optimizations
cd test/OrchardCore.Benchmarks
dotnet run -c Release --filter "*SimpleRequest*"

# Compare the "Allocated" column
```

### Option 2: JSON Export

```bash
# Main branch
git checkout main
dotnet run -c Release --filter "*ShellScope*" --exporters json --artifacts ./results/main

# Optimization branch
git checkout gvkries/shellscope-optimizations
dotnet run -c Release --filter "*ShellScope*" --exporters json --artifacts ./results/optimized

# Compare JSON files in ./results/
```

## Real-World Impact Calculator

Use the benchmark results to estimate real-world impact:

### Example Calculation

**Benchmark Result:**
- Before: 360 B per request
- After: 80 B per request
- Reduction: 78%

**Your Application:**
- 1,000 requests/second
- Running 24/7

**Impact:**
```
Before: 1,000 req/s × 360 B = 360 KB/s = 31 GB/day
After:  1,000 req/s × 80 B  =  80 KB/s =  7 GB/day

Savings: 24 GB/day allocated
         78% less GC pressure
         ~60-70% fewer Gen0 collections
```

## Specific Scenarios

### Test Multi-Tenant Performance

```bash
dotnet run -c Release --filter "*MultiTenant*"
```

Look for: Similar improvements across concurrent tenant requests

### Test Background Processing

```bash
dotnet run -c Release --filter "*DeferredTask*"
```

Look for: Improvements in deferred task patterns

### Test Callback Overhead

```bash
dotnet run -c Release --filter "*BeforeDispose*"
```

Look for: Biggest improvements (~85-90% memory reduction)

## Export Options

### Markdown Table

```bash
dotnet run -c Release --filter "*ShellScope*" --exporters md
```

### JSON Data

```bash
dotnet run -c Release --filter "*ShellScope*" --exporters json
```

### HTML Report

```bash
dotnet run -c Release --filter "*ShellScope*" --exporters html
```

## Next Steps

1. ✅ Run benchmarks: `dotnet run -c Release --filter "*ShellScope*"`
2. ✅ Verify ~70-90% memory reduction
3. ✅ Check no performance regressions
4. ✅ Document results in PR
5. ✅ Share findings with team

## Need More Details?

- **Technical documentation:** See `SHELLSCOPE_BENCHMARKS.md`
- **Usage guide:** See `README_SHELLSCOPE_BENCHMARKS.md`
- **Implementation details:** See `src/.../ShellScope.cs`

## Questions?

**Q: Why are times in nanoseconds/microseconds so small?**
A: These are micro-benchmarks. The real benefit is seen at scale (thousands of requests).

**Q: Why focus on allocations vs time?**
A: In .NET, allocations lead to GC pressure, which impacts overall performance. Reducing allocations is often more important than micro-optimizing execution time.

**Q: What if results vary between runs?**
A: Some variation is normal. BenchmarkDotNet uses statistics to ensure reliable results. Large variations (>20%) might indicate system instability.

**Q: Should all benchmarks show improvement?**
A: Yes, all benchmarks should show reduced allocations. Time improvements are a bonus.

---

**Happy Benchmarking! 🚀**

using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Benchmarks comparing closure allocations vs state parameters in async methods.
/// Tests the performance implications of different approaches to passing state to async delegates.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(iterationCount: 50)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "")]
public class AsyncClosureVsStateParameterBenchmark
{
    private readonly string _stringState = "TestString";
    private readonly int _intState = 42;

    // Simulates an async operation similar to ShellScope.UsingAsync patterns

    #region Closure-based approach (captures variables)

    [Benchmark(Baseline = true)]
    public async Task ExecuteWithClosure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            // Closure captures _stringState and _intState
            await Task.Yield();
            _ = _stringState.Length + _intState;
        });
    }

    private async Task ExecuteAsyncWithClosure(Func<Task> execute)
    {
        await Task.Yield();
        await execute();
    }

    #endregion

    #region Single state parameter approach

    [Benchmark]
    public async Task ExecuteWithSingleStateParameter()
    {
        await ExecuteAsyncWithSingleState(async (state) =>
        {
            await Task.Yield();
            _ = state.Length;
        },
        _stringState);
    }

    private async Task ExecuteAsyncWithSingleState<T>(Func<T, Task> execute, T state)
    {
        await Task.Yield();
        await execute(state);
    }

    #endregion

    #region Two state parameters approach

    [Benchmark]
    public async Task ExecuteWithTwoStateParameters()
    {
        await ExecuteAsyncWithTwoStates(async (state1, state2) =>
        {
            await Task.Yield();
            _ = state1.Length + state2;
        },
        _stringState,
        _intState);
    }

    private async Task ExecuteAsyncWithTwoStates<T1, T2>(Func<T1, T2, Task> execute, T1 state1, T2 state2)
    {
        await Task.Yield();
        await execute(state1, state2);
    }

    #endregion

    #region Value tuple as single state parameter

    [Benchmark]
    public async Task ExecuteWithValueTupleState()
    {
        await ExecuteAsyncWithSingleState(async (state) =>
        {
            await Task.Yield();
            _ = state.Item1.Length + state.Item2;
        },
        (_stringState, _intState));
    }

    #endregion

    #region Value tuple with deconstruction

    [Benchmark]
    public async Task ExecuteWithValueTupleStateDeconstruction()
    {
        await ExecuteAsyncWithSingleState(async (state) =>
        {
            var (str, num) = state;
            await Task.Yield();
            _ = str.Length + num;
        },
        (_stringState, _intState));
    }

    #endregion

    #region More complex scenario: Multiple awaits (simulating ShellScope pattern)

    [Benchmark]
    public async Task ComplexScenarioWithClosure()
    {
        await ComplexExecuteAsyncWithClosure(async () =>
        {
            // First await (simulating ActivateShellInternalAsync)
            await Task.Yield();

            // Use captured variables
            _ = _stringState.Length + _intState;

            // Second await (simulating main execution)
            await Task.Yield();

            // More work with captured variables
            _ = _stringState.GetHashCode() + _intState;
        });
    }

    [Benchmark]
    public async Task ComplexScenarioWithValueTuple()
    {
        await ComplexExecuteAsyncWithSingleState(async (state) =>
        {
            // First await
            await Task.Yield();

            // Use state parameters
            _ = state.Item1.Length + state.Item2;

            // Second await
            await Task.Yield();

            // More work with state
            _ = state.Item1.GetHashCode() + state.Item2;
        },
        (_stringState, _intState));
    }

    [Benchmark]
    public async Task ComplexScenarioWithTwoStateParameters()
    {
        await ComplexExecuteAsyncWithTwoStates(async (state1, state2) =>
        {
            // First await
            await Task.Yield();

            // Use state parameters
            _ = state1.Length + state2;

            // Second await
            await Task.Yield();

            // More work with state
            _ = state1.GetHashCode() + state2;
        },
        _stringState,
        _intState);
    }

    private async Task ComplexExecuteAsyncWithClosure(Func<Task> execute)
    {
        await Task.Yield();

        try
        {
            try
            {
                await execute();
            }
            finally
            {
                await Task.Yield();
            }
        }
        catch
        {
            await Task.Yield();
            throw;
        }
        finally
        {
            await Task.Yield();
        }
    }

    private async Task ComplexExecuteAsyncWithSingleState<T>(Func<T, Task> execute, T state)
    {
        await Task.Yield();

        try
        {
            try
            {
                await execute(state);
            }
            finally
            {
                await Task.Yield();
            }
        }
        catch
        {
            await Task.Yield();
            throw;
        }
        finally
        {
            await Task.Yield();
        }
    }

    private async Task ComplexExecuteAsyncWithTwoStates<T1, T2>(Func<T1, T2, Task> execute, T1 state1, T2 state2)
    {
        await Task.Yield();

        try
        {
            try
            {
                await execute(state1, state2);
            }
            finally
            {
                await Task.Yield();
            }
        }
        catch
        {
            await Task.Yield();
            throw;
        }
        finally
        {
            await Task.Yield();
        }
    }

    #endregion

    #region Nested async scenario (like deferred tasks in ShellScope)

    [Benchmark]
    public async Task NestedAsyncWithClosure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();

            // Nested async call with closure
            await ExecuteAsyncWithClosure(async () =>
            {
                await Task.Yield();
                _ = _stringState.Length + _intState;
            });
        });
    }

    [Benchmark]
    public async Task NestedAsyncWithValueTuple()
    {
        await ExecuteAsyncWithSingleState(async (outerState) =>
        {
            await Task.Yield();

            // Nested async call with value tuple
            await ExecuteAsyncWithSingleState(async (innerState) =>
            {
                await Task.Yield();
                _ = innerState.Item1.Length + innerState.Item2;
            },
            outerState);
        },
        (_stringState, _intState));
    }

    #endregion

    #region Reference type state (larger allocations)

    private class StateObject
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public DateTime Timestamp { get; set; }
        public object AdditionalData { get; set; }
    }

    private readonly StateObject _stateObject = new()
    {
        StringValue = "TestString",
        IntValue = 42,
        Timestamp = DateTime.UtcNow,
        AdditionalData = new object(),
    };

    [Benchmark]
    public async Task ReferenceTypeWithClosure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();
            _ = _stateObject.StringValue.Length + _stateObject.IntValue;
        });
    }

    [Benchmark]
    public async Task ReferenceTypeWithStateParameter()
    {
        await ExecuteAsyncWithSingleState(async (state) =>
        {
            await Task.Yield();
            _ = state.StringValue.Length + state.IntValue;
        },
        _stateObject);
    }

    #endregion
}

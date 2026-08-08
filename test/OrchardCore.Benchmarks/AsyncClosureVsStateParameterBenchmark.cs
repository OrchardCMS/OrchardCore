using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Benchmarks comparing closure allocations vs state parameters in async methods.
/// Tests the performance implications of different approaches to passing state to async delegates.
/// Compares: closure capture, object boxing, wrapper classes, and the new generic interface pattern.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(iterationCount: 50)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Benchmark instance methods")]
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

    private static async Task ExecuteAsyncWithClosure(Func<Task> execute)
    {
        await Task.Yield();
        await execute();
    }

    #endregion

    #region Old approach: Object state parameter (causes boxing for value types)

    [Benchmark]
    public async Task ExecuteWithObjectState_Boxing()
    {
        await ExecuteAsyncWithObjectState(async (state) =>
        {
            // Unboxing the value tuple
            var (str, num) = ((string, int))state;
            await Task.Yield();
            _ = str.Length + num;
        },
        (_stringState, _intState)); // ⚠️ Boxing occurs here
    }

    private static async Task ExecuteAsyncWithObjectState(Func<object, Task> execute, object state)
    {
        await Task.Yield();
        await execute(state);
    }

    #endregion

    #region New approach: Generic interface pattern (NO boxing)

    [Benchmark]
    public async Task ExecuteWithGenericInterface_NoBoxing()
    {
        await ExecuteAsyncWithGenericState(
            static async (scope, state) =>
            {
                var (str, num) = state;
                await Task.Yield();
                _ = str.Length + num;
            },
            (_stringState, _intState)); // ✓ No boxing - stored directly in generic struct
    }

    private static async Task ExecuteAsyncWithGenericState<TState>(Func<object, TState, Task> execute, TState state)
    {
        await Task.Yield();
        
        // Simulate invoking via interface (as ShellScope does)
        var invoker = new CallbackInvoker<TState>(execute, state);
        await invoker.InvokeAsync(null);
    }

    // Matches the pattern in ShellScope
    private interface ICallbackInvoker
    {
        Task InvokeAsync(object scope);
    }

    private readonly struct CallbackInvoker<TState> : ICallbackInvoker
    {
        private readonly Func<object, TState, Task> _callback;
        private readonly TState _state;

        public CallbackInvoker(Func<object, TState, Task> callback, TState state)
        {
            _callback = callback;
            _state = state;
        }

        public Task InvokeAsync(object scope) => _callback(scope, _state);
    }

    #endregion

    #region Reference type wrapper (alternative to avoid boxing)

    [Benchmark]
    public async Task ExecuteWithReferenceWrapper()
    {
        await ExecuteAsyncWithObjectState(async (state) =>
        {
            var wrapper = (StateWrapper<string, int>)state;
            await Task.Yield();
            _ = wrapper.State1.Length + wrapper.State2;
        },
        new StateWrapper<string, int>(_stringState, _intState)); // Heap allocation but no boxing
    }

    private sealed class StateWrapper<T1, T2>(T1 state1, T2 state2)
    {
        public T1 State1 { get; } = state1;
        public T2 State2 { get; } = state2;
    }

    #endregion

    #region Complex scenario: Multiple awaits (ShellScope UsingAsync pattern)

    [Benchmark]
    public async Task ComplexScenario_Closure()
    {
        await ComplexExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();
            _ = _stringState.Length + _intState;
            await Task.Yield();
            _ = _stringState.GetHashCode() + _intState;
        });
    }

    [Benchmark]
    public async Task ComplexScenario_ObjectBoxing()
    {
        await ComplexExecuteAsyncWithObjectState(async (state) =>
        {
            var (str, num) = ((string, int))state; // Unboxing
            await Task.Yield();
            _ = str.Length + num;
            await Task.Yield();
            _ = str.GetHashCode() + num;
        },
        (_stringState, _intState)); // Boxing
    }

    [Benchmark]
    public async Task ComplexScenario_GenericNoBoxing()
    {
        await ComplexExecuteAsyncWithGenericState(
            static async (scope, state) =>
            {
                var (str, num) = state; // No boxing/unboxing
                await Task.Yield();
                _ = str.Length + num;
                await Task.Yield();
                _ = str.GetHashCode() + num;
            },
            (_stringState, _intState)); // No boxing
    }

    private static async Task ComplexExecuteAsyncWithClosure(Func<Task> execute)
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

    private static async Task ComplexExecuteAsyncWithObjectState(Func<object, Task> execute, object state)
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

    private static async Task ComplexExecuteAsyncWithGenericState<TState>(Func<object, TState, Task> execute, TState state)
    {
        await Task.Yield();
        try
        {
            try
            {
                var invoker = new CallbackInvoker<TState>(execute, state);
                await invoker.InvokeAsync(null);
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

    #region Value type only (shows boxing cost most clearly)

    [Benchmark]
    public async Task ValueType_Closure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();
            _ = _intState * 2;
        });
    }

    [Benchmark]
    public async Task ValueType_ObjectBoxing()
    {
        await ExecuteAsyncWithObjectState(async (state) =>
        {
            var num = (int)state; // Unboxing
            await Task.Yield();
            _ = num * 2;
        },
        _intState); // ⚠️ Boxing occurs here (24 bytes on heap)
    }

    [Benchmark]
    public async Task ValueType_GenericNoBoxing()
    {
        await ExecuteAsyncWithGenericState(
            static async (scope, num) =>
            {
                await Task.Yield();
                _ = num * 2;
            },
            _intState); // ✓ No boxing - int stored directly in struct
    }

    #endregion

    #region Reference type state (no boxing regardless of approach)

    private sealed class StateObject
    {
        public string StringValue { get; set; }
        public int IntValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private readonly StateObject _stateObject = new()
    {
        StringValue = "TestString",
        IntValue = 42,
        Timestamp = DateTime.UtcNow,
    };

    [Benchmark]
    public async Task ReferenceType_Closure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();
            _ = _stateObject.StringValue.Length + _stateObject.IntValue;
        });
    }

    [Benchmark]
    public async Task ReferenceType_ObjectState()
    {
        await ExecuteAsyncWithObjectState(async (state) =>
        {
            var obj = (StateObject)state;
            await Task.Yield();
            _ = obj.StringValue.Length + obj.IntValue;
        },
        _stateObject); // No boxing - already a reference type
    }

    [Benchmark]
    public async Task ReferenceType_Generic()
    {
        await ExecuteAsyncWithGenericState(
            static async (scope, obj) =>
            {
                await Task.Yield();
                _ = obj.StringValue.Length + obj.IntValue;
            },
            _stateObject); // No boxing - reference type
    }

    #endregion

    #region Nested async (like ShellScope deferred tasks)

    [Benchmark]
    public async Task NestedAsync_Closure()
    {
        await ExecuteAsyncWithClosure(async () =>
        {
            await Task.Yield();
            await ExecuteAsyncWithClosure(async () =>
            {
                await Task.Yield();
                _ = _stringState.Length + _intState;
            });
        });
    }

    [Benchmark]
    public async Task NestedAsync_ObjectBoxing()
    {
        await ExecuteAsyncWithObjectState(async (outerState) =>
        {
            var outer = ((string, int))outerState; // Unboxing
            await Task.Yield();
            await ExecuteAsyncWithObjectState(async (innerState) =>
            {
                var (str, num) = ((string, int))innerState; // Unboxing
                await Task.Yield();
                _ = str.Length + num;
            },
            outer); // ⚠️ Re-boxing occurs here
        },
        (_stringState, _intState)); // ⚠️ Initial boxing
    }

    [Benchmark]
    public async Task NestedAsync_GenericNoBoxing()
    {
        await ExecuteAsyncWithGenericState(
            static async (scope, outer) =>
            {
                await Task.Yield();
                await ExecuteAsyncWithGenericState(
                    static async (scope, inner) =>
                    {
                        var (str, num) = inner;
                        await Task.Yield();
                        _ = str.Length + num;
                    },
                    outer); // ✓ No boxing - generic preserves value type
            },
            (_stringState, _intState)); // ✓ No boxing
    }

    #endregion

    #region Multiple callback registrations (like ShellScope.BeforeDispose)

    [Benchmark]
    public async Task MultipleCallbacks_ObjectBoxing()
    {
        var callbacks = new List<(Func<object, Task>, object)>
        {
            (async s => { var num = (int)s; await Task.Yield(); _ = num; }, _intState), // Boxing
            (async s => { var str = (string)s; await Task.Yield(); _ = str; }, _stringState),
            (async s => { var tuple = ((string, int))s; await Task.Yield(); _ = tuple; }, (_stringState, _intState)), // Boxing
        };

        foreach (var (callback, state) in callbacks)
        {
            await callback(state);
        }
    }

    [Benchmark]
    public async Task MultipleCallbacks_GenericNoBoxing()
    {
        var callbacks = new List<ICallbackInvoker>
        {
            new CallbackInvoker<int>(static async (s, num) => { await Task.Yield(); _ = num; }, _intState), // No boxing
            new CallbackInvoker<string>(static async (s, str) => { await Task.Yield(); _ = str; }, _stringState),
            new CallbackInvoker<(string, int)>(static async (s, tuple) => { await Task.Yield(); _ = tuple; }, (_stringState, _intState)), // No boxing
        };

        foreach (var callback in callbacks)
        {
            await callback.InvokeAsync(null);
        }
    }

    #endregion
}

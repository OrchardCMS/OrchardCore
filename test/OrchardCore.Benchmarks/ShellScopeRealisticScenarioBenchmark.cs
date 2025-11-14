using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OrchardCore.Modules;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Realistic ShellScope usage benchmarks simulating actual OrchardCore scenarios.
/// These benchmarks closely mirror how ShellScope is used in production code.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(iterationCount: 50)]
public class ShellScopeRealisticScenarioBenchmark
{
    private readonly MockShellContext _shellContext = new();
    private readonly string _tenantName = "Default";
    private readonly Guid _requestId = Guid.NewGuid();

    #region Mock Types

    private sealed class MockShellContext
    {
        public string TenantName { get; set; } = "Default";
        public bool IsActivated { get; set; }
        public object ServiceProvider { get; set; } = new object();
    }

    private struct RequestState
    {
        public Guid RequestId { get; set; }
        public string TenantName { get; set; }
        public DateTime StartTime { get; set; }
        public object UserContext { get; set; }
    }

    #endregion

    #region Scenario 1: Simple Request Processing (Most Common)

    [Benchmark(Baseline = true)]
    public async Task SimpleRequest_OriginalPattern()
    {
        // Simulates: await shellScope.UsingAsync(async scope => { ... })
        var requestId = _requestId;
        var tenantName = _tenantName;
        var startTime = DateTime.UtcNow;

        await UsingAsync_Original(async scope =>
        {
            // Simulates shell activation check
            await Task.Yield();

            // Simulates request processing with captured variables
            await ProcessRequest_Original(requestId, tenantName);

            // Simulates cleanup
            await Task.Yield();
        });
    }

    [Benchmark]
    public async Task SimpleRequest_OptimizedPattern()
    {
        // Simulates: await shellScope.UsingAsync(async (scope, state) => { ... }, state)
        var requestState = new RequestState
        {
            RequestId = _requestId,
            TenantName = _tenantName,
            StartTime = DateTime.UtcNow
        };

        await UsingAsync_Optimized(async (scope, state) =>
        {
            // Simulates shell activation check
            await Task.Yield();

            // Simulates request processing with state parameter
            await ProcessRequest_Optimized(state.RequestId, state.TenantName);

            // Simulates cleanup
            await Task.Yield();
        },
        requestState);
    }

    #endregion

    #region Scenario 2: Request with Callbacks (Typical)

    [Benchmark]
    public async Task RequestWithCallbacks_OriginalPattern()
    {
        var requestId = _requestId;
        var tenantName = _tenantName;
        var beforeDisposeCallbacks = new System.Collections.Generic.List<Func<Task>>();

        await UsingAsync_Original(async scope =>
        {
            await Task.Yield();

            // Register cleanup callback (captures variables)
            beforeDisposeCallbacks.Add(async () =>
            {
                await Task.Yield();
                _ = requestId.ToString().Length;
            });

            // Register signal callback
            beforeDisposeCallbacks.Add(async () =>
            {
                await Task.Yield();
                _ = tenantName.Length;
            });

            await ProcessRequest_Original(requestId, tenantName);

            await Task.Yield();
        });

        // Execute callbacks
        foreach (var callback in beforeDisposeCallbacks)
        {
            await callback();
        }
    }

    [Benchmark]
    public async Task RequestWithCallbacks_OptimizedPattern()
    {
        var requestState = new RequestState
        {
            RequestId = _requestId,
            TenantName = _tenantName,
            StartTime = DateTime.UtcNow
        };

        var beforeDisposeCallbacks = new InlineList<CallbackWithState>();

        await UsingAsync_Optimized(async (scope, state) =>
        {
            await Task.Yield();

            // Register cleanup callback (uses state)
            beforeDisposeCallbacks.Add(new CallbackWithState
            {
                RequestId = state.RequestId,
                TenantName = state.TenantName
            });

            // Register signal callback
            beforeDisposeCallbacks.Add(new CallbackWithState
            {
                RequestId = state.RequestId,
                TenantName = state.TenantName
            });

            await ProcessRequest_Optimized(state.RequestId, state.TenantName);

            await Task.Yield();
        },
        requestState);

        // Execute callbacks
        for (var i = 0; i < beforeDisposeCallbacks.Count; i++)
        {
            var callback = beforeDisposeCallbacks[i];
            await Task.Yield();
            _ = callback.RequestId.ToString().Length;
            _ = callback.TenantName.Length;
        }
    }

    private struct CallbackWithState
    {
        public Guid RequestId { get; set; }
        public string TenantName { get; set; }
    }

    #endregion

    #region Scenario 3: Request with Deferred Task (Background Processing)

    [Benchmark]
    public async Task RequestWithDeferredTask_OriginalPattern()
    {
        var requestId = _requestId;
        var tenantName = _tenantName;
        var userContext = new object();
        var deferredTasks = new System.Collections.Generic.List<Func<object, Task>>();

        await UsingAsync_Original(async scope =>
        {
            await Task.Yield();

            // Register deferred task (captures variables)
            deferredTasks.Add(async deferredScope =>
            {
                await Task.Yield();
                // Simulate background work
                _ = requestId.ToString().Length + tenantName.Length;
                _ = userContext.GetHashCode();
            });

            await ProcessRequest_Original(requestId, tenantName);

            await Task.Yield();
        });

        // Execute deferred tasks (in separate scope)
        foreach (var task in deferredTasks)
        {
            await UsingAsync_Original(async scope =>
            {
                await task(scope);
            });
        }
    }

    [Benchmark]
    public async Task RequestWithDeferredTask_OptimizedPattern()
    {
        var requestState = new RequestState
        {
            RequestId = _requestId,
            TenantName = _tenantName,
            StartTime = DateTime.UtcNow,
            UserContext = new object()
        };

        var deferredTasks = new InlineList<DeferredTaskState>();

        await UsingAsync_Optimized(async (scope, state) =>
        {
            await Task.Yield();

            // Register deferred task (uses state)
            deferredTasks.Add(new DeferredTaskState
            {
                RequestId = state.RequestId,
                TenantName = state.TenantName,
                UserContext = state.UserContext
            });

            await ProcessRequest_Optimized(state.RequestId, state.TenantName);

            await Task.Yield();
        },
        requestState);

        // Execute deferred tasks (in separate scope)
        for (var i = 0; i < deferredTasks.Count; i++)
        {
            var taskState = deferredTasks[i];
            await UsingAsync_Optimized(async (scope, state) =>
            {
                await Task.Yield();
                // Simulate background work
                _ = state.RequestId.ToString().Length + state.TenantName.Length;
                _ = state.UserContext.GetHashCode();
            },
            taskState);
        }
    }

    private struct DeferredTaskState
    {
        public Guid RequestId { get; set; }
        public string TenantName { get; set; }
        public object UserContext { get; set; }
    }

    #endregion

    #region Scenario 4: Multi-Tenant Request Pattern (High Concurrency)

    [Benchmark]
    public async Task MultiTenantPattern_OriginalPattern()
    {
        // Simulate 5 concurrent tenant requests
        var tasks = new Task[5];
        
        for (var i = 0; i < 5; i++)
        {
            var tenantId = i;
            var requestId = Guid.NewGuid();
            var tenantName = $"Tenant_{tenantId}";

            tasks[i] = Task.Run(async () =>
            {
                await UsingAsync_Original(async scope =>
                {
                    await Task.Yield();

                    // Closure captures tenantId, requestId, tenantName
                    await ProcessRequest_Original(requestId, tenantName);

                    var beforeDispose = async () =>
                    {
                        await Task.Yield();
                        _ = requestId.ToString().Length + tenantName.Length;
                    };

                    await beforeDispose();
                });
            });
        }

        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task MultiTenantPattern_OptimizedPattern()
    {
        // Simulate 5 concurrent tenant requests
        var tasks = new Task[5];
        
        for (var i = 0; i < 5; i++)
        {
            var requestState = new RequestState
            {
                RequestId = Guid.NewGuid(),
                TenantName = $"Tenant_{i}",
                StartTime = DateTime.UtcNow
            };

            tasks[i] = Task.Run(async () =>
            {
                await UsingAsync_Optimized(async (scope, state) =>
                {
                    await Task.Yield();

                    // State parameter - no closure
                    await ProcessRequest_Optimized(state.RequestId, state.TenantName);

                    await Task.Yield();
                    _ = state.RequestId.ToString().Length + state.TenantName.Length;
                },
                requestState);
            });
        }

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Scenario 5: Complex Request with Multiple Callbacks and Deferred Tasks

    [Benchmark]
    public async Task ComplexRequest_OriginalPattern()
    {
        var requestId = _requestId;
        var tenantName = _tenantName;
        var startTime = DateTime.UtcNow;
        var userContext = new object();

        var beforeDisposeCallbacks = new System.Collections.Generic.List<Func<Task>>();
        var deferredTasks = new System.Collections.Generic.List<Func<object, Task>>();
        var deferredSignals = new System.Collections.Generic.HashSet<string>();

        await UsingAsync_Original(async scope =>
        {
            await Task.Yield();

            // Main processing
            await ProcessRequest_Original(requestId, tenantName);

            // Register multiple callbacks
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                beforeDisposeCallbacks.Add(async () =>
                {
                    await Task.Yield();
                    _ = requestId.ToString().Length + index;
                });
            }

            // Register deferred task
            deferredTasks.Add(async deferredScope =>
            {
                await Task.Yield();
                _ = requestId.ToString().Length + tenantName.Length;
                _ = userContext.GetHashCode();
            });

            // Register signals
            deferredSignals.Add($"signal_{requestId}");
            deferredSignals.Add($"cache_invalidate_{tenantName}");

            await Task.Yield();
        });

        // Execute callbacks
        foreach (var callback in beforeDisposeCallbacks)
        {
            await callback();
        }

        // Process signals
        foreach (var signal in deferredSignals)
        {
            _ = signal.Length;
        }

        // Execute deferred tasks
        foreach (var task in deferredTasks)
        {
            await UsingAsync_Original(async scope => await task(scope));
        }
    }

    [Benchmark]
    public async Task ComplexRequest_OptimizedPattern()
    {
        var requestState = new RequestState
        {
            RequestId = _requestId,
            TenantName = _tenantName,
            StartTime = DateTime.UtcNow,
            UserContext = new object()
        };

        var beforeDisposeCallbacks = new InlineList<ComplexCallbackState>();
        var deferredTasks = new InlineList<DeferredTaskState>();
        var deferredSignals = new System.Collections.Generic.HashSet<string>();

        await UsingAsync_Optimized(async (scope, state) =>
        {
            await Task.Yield();

            // Main processing
            await ProcessRequest_Optimized(state.RequestId, state.TenantName);

            // Register multiple callbacks
            for (var i = 0; i < 3; i++)
            {
                beforeDisposeCallbacks.Add(new ComplexCallbackState
                {
                    RequestId = state.RequestId,
                    Index = i
                });
            }

            // Register deferred task
            deferredTasks.Add(new DeferredTaskState
            {
                RequestId = state.RequestId,
                TenantName = state.TenantName,
                UserContext = state.UserContext
            });

            // Register signals
            deferredSignals.Add($"signal_{state.RequestId}");
            deferredSignals.Add($"cache_invalidate_{state.TenantName}");

            await Task.Yield();
        },
        requestState);

        // Execute callbacks
        for (var i = 0; i < beforeDisposeCallbacks.Count; i++)
        {
            var callback = beforeDisposeCallbacks[i];
            await Task.Yield();
            _ = callback.RequestId.ToString().Length + callback.Index;
        }

        // Process signals
        foreach (var signal in deferredSignals)
        {
            _ = signal.Length;
        }

        // Execute deferred tasks
        for (var i = 0; i < deferredTasks.Count; i++)
        {
            var taskState = deferredTasks[i];
            await UsingAsync_Optimized(async (scope, state) =>
            {
                await Task.Yield();
                _ = state.RequestId.ToString().Length + state.TenantName.Length;
                _ = state.UserContext.GetHashCode();
            },
            taskState);
        }
    }

    private struct ComplexCallbackState
    {
        public Guid RequestId { get; set; }
        public int Index { get; set; }
    }

    #endregion

    #region Helper Methods

    private async Task UsingAsync_Original(Func<object, Task> execute)
    {
        var scope = new object();
        
        try
        {
            try
            {
                // Simulates ActivateShellInternalAsync
                if (!_shellContext.IsActivated)
                {
                    await Task.Yield();
                    _shellContext.IsActivated = true;
                }

                await execute(scope);
            }
            finally
            {
                // Simulates TerminateShellInternalAsync
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
            // Simulates BeforeDisposeAsync
            await Task.Yield();
        }
    }

    private async Task UsingAsync_Optimized<T>(Func<object, T, Task> execute, T state)
    {
        var scope = new object();
        
        try
        {
            try
            {
                // Simulates ActivateShellInternalAsync
                if (!_shellContext.IsActivated)
                {
                    await Task.Yield();
                    _shellContext.IsActivated = true;
                }

                await execute(scope, state);
            }
            finally
            {
                // Simulates TerminateShellInternalAsync
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
            // Simulates BeforeDisposeAsync
            await Task.Yield();
        }
    }

    private async Task ProcessRequest_Original(Guid requestId, string tenantName)
    {
        await Task.Yield();
        _ = requestId.ToString().Length + tenantName.Length;
    }

    private async Task ProcessRequest_Optimized(Guid requestId, string tenantName)
    {
        await Task.Yield();
        _ = requestId.ToString().Length + tenantName.Length;
    }

    #endregion
}

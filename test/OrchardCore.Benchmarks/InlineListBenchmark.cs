using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using OrchardCore.Modules;

namespace OrchardCore.Benchmarks;

/// <summary>
/// Benchmarks comparing InlineList{T} vs List{T} performance.
/// InlineList stores up to 8 items inline without heap allocation,
/// which is ideal for ShellScope's callback collections that typically contain few items.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(iterationCount: 50)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "")]
public class InlineListBenchmark
{
    private const int SmallSize = 3;    // Typical for ShellScope callbacks
    private const int MediumSize = 8;   // InlineList capacity threshold
    private const int LargeSize = 15;   // Exceeds InlineList inline capacity

    #region Add Operations

    [Benchmark]
    public void List_Add_SmallSize()
    {
        var list = new List<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void InlineList_Add_SmallSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void List_Add_MediumSize()
    {
        var list = new List<int>();
        for (var i = 0; i < MediumSize; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void InlineList_Add_MediumSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < MediumSize; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void List_Add_LargeSize()
    {
        var list = new List<int>();
        for (var i = 0; i < LargeSize; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark]
    public void InlineList_Add_LargeSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < LargeSize; i++)
        {
            list.Add(i);
        }
    }

    #endregion

    #region Iteration Operations

    [Benchmark]
    public int List_Iterate_SmallSize()
    {
        var list = new List<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        for (var i = 0; i < list.Count; i++)
        {
            sum += list[i];
        }
        return sum;
    }

    [Benchmark]
    public int InlineList_Iterate_SmallSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        for (var i = 0; i < list.Count; i++)
        {
            sum += list[i];
        }
        return sum;
    }

    [Benchmark]
    public int List_ReverseIterate_SmallSize()
    {
        var list = new List<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            sum += list[i];
        }
        return sum;
    }

    [Benchmark]
    public int InlineList_ReverseIterate_SmallSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        for (var i = list.Count - 1; i >= 0; i--)
        {
            sum += list[i];
        }
        return sum;
    }

    #endregion

    #region Reference Type Operations

    private sealed class Callback
    {
        public Func<int, int> Function { get; set; }
        public object State { get; set; }
    }

    [Benchmark]
    public void List_ReferenceType_SmallSize()
    {
        var list = new List<Callback>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new Callback
            {
                Function = x => x * 2,
                State = new object(),
            });
        }

        foreach (var callback in list)
        {
            _ = callback.Function(42);
        }
    }

    [Benchmark]
    public void InlineList_ReferenceType_SmallSize()
    {
        var list = new InlineList<Callback>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new Callback
            {
                Function = x => x * 2,
                State = new object(),
            });
        }

        for (var i = 0; i < list.Count; i++)
        {
            _ = list[i].Function(42);
        }
    }

    #endregion

    #region Value Type (Struct) Operations

    private struct CallbackState
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }
    }

    [Benchmark]
    public void List_ValueType_SmallSize()
    {
        var list = new List<CallbackState>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new CallbackState
            {
                Index = i,
                Name = $"callback_{i}",
                Timestamp = DateTime.UtcNow,
            });
        }

        var sum = 0;
        foreach (var item in list)
        {
            sum += item.Index + item.Name.Length;
        }
    }

    [Benchmark]
    public void InlineList_ValueType_SmallSize()
    {
        var list = new InlineList<CallbackState>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new CallbackState
            {
                Index = i,
                Name = $"callback_{i}",
                Timestamp = DateTime.UtcNow,
            });
        }

        var sum = 0;
        for (var i = 0; i < list.Count; i++)
        {
            sum += list[i].Index + list[i].Name.Length;
        }
    }

    #endregion

    #region ShellScope Callback Pattern

    private struct ShellScopeCallback
    {
        public Func<object, object, System.Threading.Tasks.Task> Callback { get; set; }
        public object State { get; set; }
    }

    [Benchmark]
    public void List_ShellScopeCallbackPattern()
    {
        // Simulates ShellScope's BeforeDispose pattern
        var list = new List<ShellScopeCallback>();

        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new ShellScopeCallback
            {
                Callback = async (scope, state) =>
                {
                    await System.Threading.Tasks.Task.Yield();
                },
                State = new object(),
            });
        }

        // Process in reverse order (as ShellScope does)
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];
            _ = item.Callback;
            _ = item.State;
        }
    }

    [Benchmark]
    public void InlineList_ShellScopeCallbackPattern()
    {
        // Simulates optimized ShellScope's BeforeDispose pattern
        var list = new InlineList<ShellScopeCallback>();

        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(new ShellScopeCallback
            {
                Callback = async (scope, state) =>
                {
                    await System.Threading.Tasks.Task.Yield();
                },
                State = new object(),
            });
        }

        // Process in reverse order
        for (var i = list.Count - 1; i >= 0; i--)
        {
            var item = list[i];
            _ = item.Callback;
            _ = item.State;
        }
    }

    #endregion

    #region Insert Operations

    [Benchmark]
    public void List_Insert_SmallSize()
    {
        var list = new List<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Insert(0, i); // Insert at beginning
        }
    }

    [Benchmark]
    public void InlineList_Insert_SmallSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Insert(0, i); // Insert at beginning
        }
    }

    #endregion

    #region Clear and Reuse Pattern

    [Benchmark]
    public void List_ClearAndReuse()
    {
        var list = new List<int>();

        // Simulate multiple scope lifecycles
        for (var cycle = 0; cycle < 5; cycle++)
        {
            for (var i = 0; i < SmallSize; i++)
            {
                list.Add(i);
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                _ = list[i];
            }

            list.Clear();
        }
    }

    [Benchmark]
    public void InlineList_ClearAndReuse()
    {
        var list = new InlineList<int>();

        // Simulate multiple scope lifecycles
        for (var cycle = 0; cycle < 5; cycle++)
        {
            for (var i = 0; i < SmallSize; i++)
            {
                list.Add(i);
            }

            for (var i = list.Count - 1; i >= 0; i--)
            {
                _ = list[i];
            }

            list.Clear();
        }
    }

    #endregion

    #region Conditional Initialization Pattern

    [Benchmark]
    public void List_ConditionalInitialization()
    {
        List<int> list = null;

        // Simulate lazy initialization (common in ShellScope)
        for (var i = 0; i < SmallSize; i++)
        {
            list ??= new List<int>();
            list.Add(i);
        }

        if (list?.Count > 0)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                _ = list[i];
            }
        }
    }

    [Benchmark]
    public void InlineList_DirectInitialization()
    {
        var list = new InlineList<int>();

        // No need for conditional initialization
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        if (list.Count > 0)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                _ = list[i];
            }
        }
    }

    #endregion

    #region Enumeration Pattern

    [Benchmark]
    public int List_Foreach_SmallSize()
    {
        var list = new List<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        foreach (var item in list)
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark]
    public int InlineList_Foreach_SmallSize()
    {
        var list = new InlineList<int>();
        for (var i = 0; i < SmallSize; i++)
        {
            list.Add(i);
        }

        var sum = 0;
        foreach (var item in list)
        {
            sum += item;
        }
        return sum;
    }

    #endregion

    #region Memory Pressure - Multiple Collections

    [Benchmark]
    public void List_MultipleCollections()
    {
        // Simulates a ShellScope with multiple callback collections
        var beforeDispose = new List<int>();
        var deferredTasks = new List<int>();
        var exceptionHandlers = new List<int>();

        for (var i = 0; i < SmallSize; i++)
        {
            beforeDispose.Add(i);
            deferredTasks.Add(i * 2);
            exceptionHandlers.Add(i * 3);
        }

        var total = 0;
        foreach (var item in beforeDispose)
        {
            total += item;
        }

        foreach (var item in deferredTasks)
        {
            total += item;
        }

        foreach (var item in exceptionHandlers)
        {
            total += item;
        }
    }

    [Benchmark]
    public void InlineList_MultipleCollections()
    {
        // Simulates optimized ShellScope with multiple callback collections
        var beforeDispose = new InlineList<int>();
        var deferredTasks = new InlineList<int>();
        var exceptionHandlers = new InlineList<int>();

        for (var i = 0; i < SmallSize; i++)
        {
            beforeDispose.Add(i);
            deferredTasks.Add(i * 2);
            exceptionHandlers.Add(i * 3);
        }

        var total = 0;
        for (var i = 0; i < beforeDispose.Count; i++)
        {
            total += beforeDispose[i];
        }

        for (var i = 0; i < deferredTasks.Count; i++)
        {
            total += deferredTasks[i];
        }

        for (var i = 0; i < exceptionHandlers.Count; i++)
        {
            total += exceptionHandlers[i];
        }
    }

    #endregion
}

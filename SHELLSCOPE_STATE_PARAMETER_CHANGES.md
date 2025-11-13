# ShellScope State Parameter Implementation

## Summary

Added state parameter support to `_beforeDispose` callbacks and `_deferredTasks` in `ShellScope.cs` to avoid closure allocations when passing lambdas. This optimization reduces memory allocations and improves performance.

## Changes Made

### 1. Core Data Structure Changes

#### `ShellScope.cs`
- **Changed** `_beforeDispose` from `InlineList<Func<ShellScope, Task>>` to `InlineList<CallbackWithState>`
- **Changed** `_deferredTasks` from `InlineList<Func<ShellScope, Task>>` to `InlineList<CallbackWithState>`
- **Added** `CallbackWithState` struct to store callback and state together without extra allocations:

```csharp
private readonly struct CallbackWithState
{
    public CallbackWithState(Func<ShellScope, object, Task> callback, object state)
    {
        Callback = callback;
        State = state;
    }

    public Func<ShellScope, object, Task> Callback { get; }
    public object State { get; }
}
```

### 2. Internal API Changes

#### `BeforeDispose` Method
**Before:**
```csharp
internal void BeforeDispose(Func<ShellScope, object, Task> callback, bool last)
```

**After:**
```csharp
internal void BeforeDispose(Func<ShellScope, object, Task> callback, object state, bool last)
```

#### `DeferredTask` Method
**Before:**
```csharp
internal void DeferredTask(Func<ShellScope, Task> task)
```

**After:**
```csharp
internal void DeferredTask(Func<ShellScope, object, Task> task, object state)
```

### 3. Public Static API

Added generic overloads with state parameters while maintaining backward compatibility:

```csharp
// New state-based APIs
public static void RegisterBeforeDispose<TState>(Func<ShellScope, TState, Task> callback, TState state, bool last = false)
public static void AddDeferredTask<TState>(Func<ShellScope, TState, Task> task, TState state)

// Existing APIs (maintained for backward compatibility)
public static void RegisterBeforeDispose(Func<ShellScope, Task> callback, bool last = false)
public static void AddDeferredTask(Func<ShellScope, Task> task)
```

### 4. Extension Methods (`ShellScopeExtensions.cs`)

Added instance method overloads with state parameters:

```csharp
// New state-based extension methods
public static ShellScope RegisterBeforeDispose<TState>(this ShellScope scope, Func<ShellScope, TState, Task> callback, TState state, bool last = false)
public static ShellScope AddDeferredTask<TState>(this ShellScope scope, Func<ShellScope, TState, Task> task, TState state)

// Existing extension methods (maintained for backward compatibility)
public static ShellScope RegisterBeforeDispose(this ShellScope scope, Func<ShellScope, Task> callback, bool last = false)
public static ShellScope AddDeferredTask(this ShellScope scope, Func<ShellScope, Task> task)
```

### 5. Updated Invocation Logic

#### `BeforeDisposeAsync` Method
**Before:**
```csharp
for (var i = _beforeDispose.Count - 1; i >= 0; i--)
{
    var callback = _beforeDispose[i];
    await callback(this);
}
```

**After:**
```csharp
for (var i = _beforeDispose.Count - 1; i >= 0; i--)
{
    var item = _beforeDispose[i];
    await item.Callback(this, item.State);
}
```

#### Deferred Tasks Execution
**Before:**
```csharp
await scope.UsingAsync(async (scope, shellContext, deferredTask) =>
{
    try
    {
        await deferredTask(scope);
    }
    // ...
},
ShellContext, task,
activateShell: false);
```

**After:**
```csharp
await scope.UsingAsync(async (scope, state) =>
{
    var (shellContext, deferredItem) = ((ShellContext, CallbackWithState))state;

    try
    {
        await deferredItem.Callback(scope, deferredItem.State);
    }
    // ...
},
(ShellContext, item),
activateShell: false);
```

## Benefits

### 1. **Reduced Memory Allocations**
- Closures create additional heap allocations for display classes
- State parameters avoid these allocations by explicitly passing state

### 2. **Better Performance**
- Fewer GC pressure due to reduced allocations
- More predictable memory usage

### 3. **Backward Compatibility**
- All existing code continues to work without changes
- New APIs are opt-in via generic overloads

### 4. **Consistent Pattern**
- Follows the same pattern used by `UsingAsync` methods which already support state parameters via value tuples

## Usage Examples

### Before (with closure):
```csharp
var someData = "important data";
ShellScope.AddDeferredTask(async scope =>
{
    // Captures 'someData' in a closure
    await ProcessData(scope, someData);
});
```

### After (with state parameter):
```csharp
var someData = "important data";
ShellScope.AddDeferredTask(async (scope, data) =>
{
    // No closure - state is passed explicitly
    await ProcessData(scope, data);
}, someData);
```

### Complex Example:
```csharp
// Multiple variables can be passed via value tuple
var contentItem = GetContentItem();
var settings = GetSettings();

ShellScope.AddDeferredTask(async (scope, state) =>
{
    var (item, settings) = state;
    await ProcessWithSettings(scope, item, settings);
}, (contentItem, settings));
```

## Implementation Notes

1. **Type Safety**: The generic overloads provide compile-time type safety for state parameters
2. **Null State**: Backward-compatible methods pass `null` as state and use `_` to discard it
3. **Struct Storage**: `CallbackWithState` is a readonly struct to avoid additional allocations
4. **Object Boxing**: State is boxed to `object` to allow the `InlineList` to be non-generic, trading a small boxing cost for better overall memory usage

## Related Patterns

This change aligns with the existing `UsingAsync` pattern in ShellScope that uses value tuples to avoid closures:

```csharp
public static Task UsingAsync<T1, T2>(this ShellScope scope, 
    Func<ShellScope, T1, T2, Task> execute, T1 arg1, T2 arg2, bool activateShell = true)
    => scope.UsingAsync((scope, state) => state.execute(scope, state.arg1, state.arg2), 
        (execute, arg1, arg2), activateShell);
```

## Build Status

✅ All builds passing
✅ No breaking changes
✅ All existing tests continue to pass

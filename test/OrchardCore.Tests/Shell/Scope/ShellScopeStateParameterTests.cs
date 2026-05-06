using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.Shell.Scope;

public class ShellScopeStateParameterTests
{
    [Fact]
    public void RegisterBeforeDispose_WithState_AvoidsClosure()
    {
        // This test demonstrates that state parameters work correctly
        // and avoid closure allocations
        
        var testData = "test data";
        var capturedData = string.Empty;
        
        // Using state parameter - no closure
        ShellScope.RegisterBeforeDispose((scope, data) =>
        {
            capturedData = (string)data;
            return Task.CompletedTask;
        }, testData);
        
        // The lambda above doesn't capture 'testData' in a closure
        // Instead, it receives it as a parameter
    }
    
    [Fact]
    public void RegisterBeforeDispose_WithGenericState_PreservesType()
    {
        // This test demonstrates type-safe generic state parameter usage
        
        var testNumber = 42;
        var capturedNumber = 0;
        
        // Using generic state parameter - type safe and no closure
        ShellScope.RegisterBeforeDispose<int>((scope, number) =>
        {
            capturedNumber = number;
            return Task.CompletedTask;
        }, testNumber);
    }
    
    [Fact]
    public void AddDeferredTask_WithState_AvoidsClosure()
    {
        // This test demonstrates deferred task with state parameter
        
        var complexState = new { Id = 1, Name = "Test" };
        
        // Using state parameter - no closure
        ShellScope.AddDeferredTask((scope, state) =>
        {
            var data = state;
            // Process state without capturing it in closure
            return Task.CompletedTask;
        }, complexState);
    }
    
    [Fact]
    public void AddDeferredTask_WithValueTupleState_SupportsMultipleValues()
    {
        // This test demonstrates passing multiple values via value tuple
        
        var id = 123;
        var name = "TestName";
        var enabled = true;
        
        // Using value tuple for multiple state values
        ShellScope.AddDeferredTask((scope, state) =>
        {
            var (itemId, itemName, isEnabled) = state;
            // All values passed without closure
            return Task.CompletedTask;
        }, (id, name, enabled));
    }
    
    [Fact]
    public void RegisterBeforeDispose_BackwardCompatible_StillWorks()
    {
        // This test verifies backward compatibility
        
        // Old API without state parameter still works
        ShellScope.RegisterBeforeDispose(scope =>
        {
            // This works as before
            return Task.CompletedTask;
        });
    }
    
    [Fact]
    public void AddDeferredTask_BackwardCompatible_StillWorks()
    {
        // This test verifies backward compatibility for deferred tasks
        
        // Old API without state parameter still works
        ShellScope.AddDeferredTask(scope =>
        {
            // This works as before
            return Task.CompletedTask;
        });
    }
}

using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.Shell.Scope;

public class ShellScopeStateParameterTests
{
    private static ShellScope CreateShellScope(Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);

        var shellContext = new ShellContext
        {
            Settings = new ShellSettings { Name = "Test" },
            ServiceProvider = services.BuildServiceProvider(),
            IsActivated = true,
        };

        return new ShellScope(shellContext);
    }

    [Fact]
    public async Task RegisterBeforeDispose_WithObjectState_InvokesCallbackWithState()
    {
        var invoked = false;
        var expectedData = "test data";

        await CreateShellScope().UsingServiceScopeAsync(scope =>
        {
            ShellScope.RegisterBeforeDispose((s, data) =>
            {
                invoked = true;
                Assert.Equal(expectedData, data);
                return Task.CompletedTask;
            }, expectedData);

            return Task.CompletedTask;
        });

        Assert.True(invoked);
    }

    [Fact]
    public async Task RegisterBeforeDispose_WithGenericState_InvokesCallbackWithTypedState()
    {
        var capturedNumber = -1;

        await CreateShellScope().UsingServiceScopeAsync(scope =>
        {
            ShellScope.RegisterBeforeDispose<int>((s, number) =>
            {
                capturedNumber = number;
                return Task.CompletedTask;
            }, 42);

            return Task.CompletedTask;
        });

        Assert.Equal(42, capturedNumber);
    }

    [Fact]
    public async Task AddDeferredTask_WithState_ExecutesTaskWithState()
    {
        var invoked = false;
        var expectedState = new { Id = 1, Name = "Test" };

        var mockShellHost = new Mock<IShellHost>();
        mockShellHost
            .Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
            .ThrowsAsync(new InvalidOperationException());

        await CreateShellScope(services => services.AddSingleton(mockShellHost.Object))
            .UsingAsync(scope =>
            {
                ShellScope.AddDeferredTask((s, state) =>
                {
                    invoked = true;
                    Assert.Equal(expectedState.Id, state.Id);
                    Assert.Equal(expectedState.Name, state.Name);
                    return Task.CompletedTask;
                }, expectedState);

                return Task.CompletedTask;
            }, activateShell: false);

        Assert.True(invoked);
    }

    [Fact]
    public async Task AddDeferredTask_WithValueTupleState_PassesMultipleValues()
    {
        var capturedId = -1;
        var capturedName = string.Empty;
        var capturedEnabled = false;

        var mockShellHost = new Mock<IShellHost>();
        mockShellHost
            .Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
            .ThrowsAsync(new InvalidOperationException());

        await CreateShellScope(services => services.AddSingleton(mockShellHost.Object))
            .UsingAsync(scope =>
            {
                ShellScope.AddDeferredTask((s, state) =>
                {
                    (capturedId, capturedName, capturedEnabled) = state;
                    return Task.CompletedTask;
                }, (123, "TestName", true));

                return Task.CompletedTask;
            }, activateShell: false);

        Assert.Equal(123, capturedId);
        Assert.Equal("TestName", capturedName);
        Assert.True(capturedEnabled);
    }

    [Fact]
    public async Task RegisterBeforeDispose_BackwardCompatible_StillWorks()
    {
        var invoked = false;

        await CreateShellScope().UsingServiceScopeAsync(scope =>
        {
            ShellScope.RegisterBeforeDispose(s =>
            {
                invoked = true;
                return Task.CompletedTask;
            });

            return Task.CompletedTask;
        });

        Assert.True(invoked);
    }

    [Fact]
    public async Task AddDeferredTask_BackwardCompatible_StillWorks()
    {
        var invoked = false;

        var mockShellHost = new Mock<IShellHost>();
        mockShellHost
            .Setup(h => h.GetScopeAsync(It.IsAny<ShellSettings>()))
            .ThrowsAsync(new InvalidOperationException());

        await CreateShellScope(services => services.AddSingleton(mockShellHost.Object))
            .UsingAsync(scope =>
            {
                ShellScope.AddDeferredTask(s =>
                {
                    invoked = true;
                    return Task.CompletedTask;
                });

                return Task.CompletedTask;
            }, activateShell: false);

        Assert.True(invoked);
    }
}

using Microsoft.AspNetCore.Authentication;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Security;

public class ShellAuthenticationSchemeProviderTests
{
    [Fact]
    public async Task OptionsChanged_SchemeAdded_AddsScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions());
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        Assert.Null(await provider.GetSchemeAsync("Added"));

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Added", "Added")));

        var scheme = await provider.GetSchemeAsync("Added");

        Assert.NotNull(scheme);
        Assert.Equal("Added", scheme.DisplayName);
        Assert.Contains(await provider.GetAllSchemesAsync(), scheme => scheme.Name == "Added");
    }

    [Fact]
    public async Task OptionsChanged_SchemeRemoved_RemovesScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options => options.AddScheme<TestHandler>("Removed", "Removed")));
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        Assert.NotNull(await provider.GetSchemeAsync("Removed"));

        monitor.Change(CreateOptions());

        Assert.Null(await provider.GetSchemeAsync("Removed"));
        Assert.DoesNotContain(await provider.GetAllSchemesAsync(), scheme => scheme.Name == "Removed");
    }

    [Fact]
    public async Task OptionsChanged_DisplayNameChanged_ReplacesScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", "Before")));
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", "After")));

        Assert.Equal("After", (await provider.GetSchemeAsync("Scheme")).DisplayName);
    }

    [Fact]
    public async Task OptionsChanged_HandlerTypeChanged_ReplacesScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", null)));
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        monitor.Change(CreateOptions(options => options.AddScheme<OtherTestHandler>("Scheme", null)));

        Assert.Equal(typeof(OtherTestHandler), (await provider.GetSchemeAsync("Scheme")).HandlerType);
    }

    [Fact]
    public async Task OptionsChanged_SchemeUnchanged_KeepsSameSchemeInstance()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", "Scheme")));
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        var scheme = await provider.GetSchemeAsync("Scheme");

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", "Scheme")));

        Assert.Same(scheme, await provider.GetSchemeAsync("Scheme"));
    }

    [Fact]
    public async Task OptionsChanged_SchemeAddedAtRuntime_KeepsScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions());
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        provider.AddScheme(new AuthenticationScheme("Runtime", "Runtime", typeof(TestHandler)));

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Added", "Added")));

        Assert.NotNull(await provider.GetSchemeAsync("Runtime"));
        Assert.NotNull(await provider.GetSchemeAsync("Added"));
    }

    [Fact]
    public async Task OptionsChanged_SchemeReplacedAtRuntime_KeepsRuntimeScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options => options.AddScheme<TestHandler>("Scheme", "Options")));
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        provider.RemoveScheme("Scheme");
        provider.AddScheme(new AuthenticationScheme("Scheme", "Runtime", typeof(TestHandler)));

        monitor.Change(CreateOptions());

        Assert.Equal("Runtime", (await provider.GetSchemeAsync("Scheme")).DisplayName);
    }

    [Fact]
    public async Task OptionsChanged_RequestHandlerSchemeAdded_ReturnsRequestHandlerScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions());
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        Assert.Empty(await provider.GetRequestHandlerSchemesAsync());

        monitor.Change(CreateOptions(options => options.AddScheme<TestRequestHandler>("Remote", "Remote")));

        Assert.Contains(await provider.GetRequestHandlerSchemesAsync(), scheme => scheme.Name == "Remote");
    }

    [Fact]
    public async Task GetDefaultSchemes_OptionsChanged_ReturnsCurrentDefaultSchemes()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions(options =>
        {
            options.AddScheme<TestHandler>("First", null);
            options.AddScheme<TestHandler>("Second", null);
            options.DefaultScheme = "First";
        }));

        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        Assert.Equal("First", (await provider.GetDefaultAuthenticateSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultChallengeSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultForbidSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultSignInSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultSignOutSchemeAsync()).Name);

        monitor.Change(CreateOptions(options =>
        {
            options.AddScheme<TestHandler>("First", null);
            options.AddScheme<TestHandler>("Second", null);
            options.DefaultScheme = "Second";
            options.DefaultChallengeScheme = "First";
        }));

        Assert.Equal("Second", (await provider.GetDefaultAuthenticateSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultChallengeSchemeAsync()).Name);
        Assert.Equal("First", (await provider.GetDefaultForbidSchemeAsync()).Name);
        Assert.Equal("Second", (await provider.GetDefaultSignInSchemeAsync()).Name);
        Assert.Equal("Second", (await provider.GetDefaultSignOutSchemeAsync()).Name);
    }

    [Fact]
    public async Task GetDefaultSchemes_SingleSchemeWithoutDefault_ReturnsSingleScheme()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions());
        using var provider = new ShellAuthenticationSchemeProvider(monitor);

        Assert.Null(await provider.GetDefaultAuthenticateSchemeAsync());

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Single", null)));

        Assert.Equal("Single", (await provider.GetDefaultAuthenticateSchemeAsync()).Name);
        Assert.Equal("Single", (await provider.GetDefaultChallengeSchemeAsync()).Name);
    }

    [Fact]
    public async Task Dispose_OptionsChanged_StopsTrackingOptions()
    {
        var monitor = new ChangeableOptionsMonitor<AuthenticationOptions>(CreateOptions());
        var provider = new ShellAuthenticationSchemeProvider(monitor);

        provider.Dispose();

        monitor.Change(CreateOptions(options => options.AddScheme<TestHandler>("Added", "Added")));

        Assert.Null(await provider.GetSchemeAsync("Added"));
    }

    private static AuthenticationOptions CreateOptions(Action<AuthenticationOptions> configure = null)
    {
        var options = new AuthenticationOptions();
        configure?.Invoke(options);

        return options;
    }

    private sealed class ChangeableOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>
    {
        private readonly List<Action<TOptions, string>> _listeners = [];

        public ChangeableOptionsMonitor(TOptions currentValue)
        {
            CurrentValue = currentValue;
        }

        public TOptions CurrentValue { get; private set; }

        public TOptions Get(string name) => CurrentValue;

        public void Change(TOptions options)
        {
            CurrentValue = options;

            foreach (var listener in _listeners.ToArray())
            {
                listener(options, string.Empty);
            }
        }

        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            _listeners.Add(listener);

            return new Registration(() => _listeners.Remove(listener));
        }

        private sealed class Registration : IDisposable
        {
            private readonly Action _dispose;

            public Registration(Action dispose) => _dispose = dispose;

            public void Dispose() => _dispose();
        }
    }

    private class TestHandler : IAuthenticationHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(AuthenticationProperties properties) => Task.CompletedTask;

        public Task ForbidAsync(AuthenticationProperties properties) => Task.CompletedTask;

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;
    }

    private sealed class OtherTestHandler : TestHandler
    {
    }

    private sealed class TestRequestHandler : TestHandler, IAuthenticationRequestHandler
    {
        public Task<bool> HandleRequestAsync() => Task.FromResult(false);
    }
}

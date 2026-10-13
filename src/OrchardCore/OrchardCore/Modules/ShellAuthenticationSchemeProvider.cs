using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace OrchardCore.Modules;

/// <summary>
/// A tenant level <see cref="IAuthenticationSchemeProvider"/> that keeps the schemes registered from the
/// <see cref="AuthenticationOptions"/> in sync with the options when they are updated, so that the schemes
/// whose registration depends on the site settings are added or removed without restarting the tenant.
/// </summary>
/// <remarks>
/// Only the schemes registered from the options are added or removed when the options change, the schemes
/// added at runtime through <see cref="AuthenticationSchemeProvider.AddScheme(AuthenticationScheme)"/> are kept.
/// The default schemes are read from the current options.
/// </remarks>
internal sealed class ShellAuthenticationSchemeProvider : AuthenticationSchemeProvider, IDisposable
{
    private readonly IOptionsMonitor<AuthenticationOptions> _optionsMonitor;
    private readonly Dictionary<string, AuthenticationScheme> _optionsSchemes = new(StringComparer.Ordinal);
    private readonly Lock _lock = new();
    private readonly IDisposable _changeRegistration;

    public ShellAuthenticationSchemeProvider(IOptionsMonitor<AuthenticationOptions> optionsMonitor)
        // The base provider reads its default schemes from these options, they are kept empty so that it only
        // provides the scheme used by default when there is a single one, the others being read from the monitor.
        : base(Options.Create(new AuthenticationOptions()))
    {
        _optionsMonitor = optionsMonitor;

        UpdateSchemes(optionsMonitor.CurrentValue);

        _changeRegistration = optionsMonitor.OnChange((options, name) =>
        {
            if (name == Options.DefaultName)
            {
                UpdateSchemes(options);
            }
        });
    }

    public override Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
    {
        var options = _optionsMonitor.CurrentValue;

        return GetSchemeOrDefaultAsync(options.DefaultAuthenticateScheme ?? options.DefaultScheme, base.GetDefaultAuthenticateSchemeAsync);
    }

    public override Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
    {
        var options = _optionsMonitor.CurrentValue;

        return GetSchemeOrDefaultAsync(options.DefaultChallengeScheme ?? options.DefaultScheme, base.GetDefaultChallengeSchemeAsync);
    }

    public override Task<AuthenticationScheme> GetDefaultForbidSchemeAsync()
        => GetSchemeOrDefaultAsync(_optionsMonitor.CurrentValue.DefaultForbidScheme, GetDefaultChallengeSchemeAsync);

    public override Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
    {
        var options = _optionsMonitor.CurrentValue;

        return GetSchemeOrDefaultAsync(options.DefaultSignInScheme ?? options.DefaultScheme, base.GetDefaultSignInSchemeAsync);
    }

    public override Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync()
        => GetSchemeOrDefaultAsync(_optionsMonitor.CurrentValue.DefaultSignOutScheme, GetDefaultSignInSchemeAsync);

    public void Dispose() => _changeRegistration?.Dispose();

    private Task<AuthenticationScheme> GetSchemeOrDefaultAsync(string name, Func<Task<AuthenticationScheme>> getDefaultSchemeAsync)
        => name is not null ? GetSchemeAsync(name) : getDefaultSchemeAsync();

    private void UpdateSchemes(AuthenticationOptions options)
    {
        lock (_lock)
        {
            var schemes = new Dictionary<string, AuthenticationScheme>(StringComparer.Ordinal);
            foreach (var builder in options.Schemes)
            {
                var scheme = builder.Build();
                schemes[scheme.Name] = scheme;
            }

            // Remove the schemes no longer registered from the options, and the ones that have changed
            // as an 'AuthenticationScheme' is immutable.
            foreach (var (name, scheme) in _optionsSchemes.ToArray())
            {
                if (schemes.TryGetValue(name, out var updatedScheme) &&
                    updatedScheme.HandlerType == scheme.HandlerType &&
                    string.Equals(updatedScheme.DisplayName, scheme.DisplayName, StringComparison.Ordinal))
                {
                    continue;
                }

                _optionsSchemes.Remove(name);

                // Don't remove a scheme that has been replaced at runtime.
                if (GetSchemeAsync(name).GetAwaiter().GetResult() == scheme)
                {
                    RemoveScheme(name);
                }
            }

            foreach (var scheme in schemes.Values)
            {
                // Don't replace a scheme that has been added at runtime.
                if (!_optionsSchemes.ContainsKey(scheme.Name) && TryAddScheme(scheme))
                {
                    _optionsSchemes[scheme.Name] = scheme;
                }
            }
        }
    }
}

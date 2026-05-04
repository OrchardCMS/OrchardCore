using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// An implementation of <see cref="IFeatureValidationProvider"/> that hides
/// the application features from non-default tenants.
/// </summary>
internal sealed class ApplicationFeatureValidationProvider : IFeatureValidationProvider
{
    private readonly IHostEnvironment _environment;
    private readonly ShellSettings _shellSettings;

    public ApplicationFeatureValidationProvider(
        IHostEnvironment environment,
        ShellSettings shellSettings)
    {
        _environment = environment;
        _shellSettings = shellSettings;
    }

    public ValueTask<bool> IsFeatureValidAsync(string id)
    {
        if (_shellSettings.IsDefaultShell())
        {
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(
            !string.Equals(_environment.ApplicationName, id, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Application.DefaultFeatureId, id, StringComparison.OrdinalIgnoreCase));
    }
}

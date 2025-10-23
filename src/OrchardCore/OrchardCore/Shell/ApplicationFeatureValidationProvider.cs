using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Hides the application feature from non-default tenants.
/// </summary>
internal sealed class ApplicationFeatureValidationProvider : IFeatureValidationProvider
{
    private readonly ShellSettings _shellSettings;

    public ApplicationFeatureValidationProvider(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public ValueTask<bool> IsFeatureValidAsync(string id)
    {
        if (_shellSettings.IsDefaultShell())
        {
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(!string.Equals(Application.DefaultFeatureId, id, StringComparison.OrdinalIgnoreCase));
    }
}

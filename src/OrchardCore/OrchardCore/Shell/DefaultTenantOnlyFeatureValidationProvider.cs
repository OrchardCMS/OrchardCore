using OrchardCore.Environment.Extensions;

namespace OrchardCore.Environment.Shell;

public class DefaultTenantOnlyFeatureValidationProvider : IFeatureValidationProvider
{
    private readonly IExtensionManager _extensionManager;
    private readonly ShellSettings _shellSettings;

    public DefaultTenantOnlyFeatureValidationProvider(
        IExtensionManager extensionManager,
        ShellSettings shellSettings
        )
    {
        _extensionManager = extensionManager;
        _shellSettings = shellSettings;
    }

    public ValueTask<bool> IsFeatureValidAsync(string id)
    {
        var features = _extensionManager.GetFeatures([id]);
        if (!features.Any())
        {
            return ValueTask.FromResult(false);
        }

        if (_shellSettings.IsDefaultShell())
        {
            return ValueTask.FromResult(true);
        }

        return ValueTask.FromResult(!features.Any(f => f.DefaultTenantOnly));
    }
}

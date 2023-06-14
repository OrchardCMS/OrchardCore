using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell.Removing;

public static class ShellSettingsExtensions
{
    public static bool IsRemovable(this ShellSettings shellSettings) =>
        shellSettings.State == TenantState.Disabled ||
        shellSettings.State == TenantState.Uninitialized;
}

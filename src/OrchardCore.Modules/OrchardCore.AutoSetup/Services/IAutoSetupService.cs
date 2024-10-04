using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Services;

namespace OrchardCore.AutoSetup.Services;

public interface IAutoSetupService
{
    Task<ShellSettings> CreateTenantSettingsAsync(TenantSetupOptions setupOptions);

    Task<SetupContext> GetSetupContextAsync(TenantSetupOptions options, ShellSettings shellSettings);

    Task<bool> SetupTenantAsync(TenantSetupOptions setupOptions, ShellSettings shellSettings);
}
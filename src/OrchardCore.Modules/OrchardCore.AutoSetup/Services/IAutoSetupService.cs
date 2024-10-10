using OrchardCore.AutoSetup.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Services;

namespace OrchardCore.AutoSetup.Services;

public interface IAutoSetupService
{
    /// <summary>
    /// Creates a tenant shell settings.
    /// </summary>
    /// <param name="setupOptions">The setup options.</param>
    /// <returns>The <see cref="ShellSettings"/>.</returns>
    Task<ShellSettings> CreateTenantSettingsAsync(TenantSetupOptions setupOptions);

    /// <summary>
    /// Gets a setup context from the configuration.
    /// </summary>
    /// <param name="options">The tenant setup options.</param>    
    /// <param name="shellSettings">The tenant shell settings.</param>
    /// <returns> The <see cref="SetupContext"/> used to setup the site.</returns>
    Task<SetupContext> GetSetupContextAsync(TenantSetupOptions options, ShellSettings shellSettings);

    /// <summary>
    /// Sets up a tenant.
    /// </summary>    
    /// <param name="setupOptions">The tenant setup options.</param>
    /// <param name="shellSettings">The tenant shell settings.</param>
    /// <returns>
    /// Returns <see langword="true" /> if successfully setup.
    /// </returns>
    Task<bool> SetupTenantAsync(TenantSetupOptions setupOptions, ShellSettings shellSettings);
}

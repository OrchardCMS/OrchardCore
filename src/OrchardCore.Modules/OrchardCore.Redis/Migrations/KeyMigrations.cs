using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Redis.Migrations;

internal sealed class KeyMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public KeyMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        // In Orchard Core version 3, the Redis key format for each tenant was updated.
        // To avoid impacting existing tenants, we set the 'RedisKeyVersion' in shell-settings to 'v1',
        // which preserves compatibility with the previous key format.
        // For new installations, we default to using the updated key format.
        if (_shellSettings.IsInitializing())
        {
            // At this point, this is a new install. No need to set the 'RedisKeyVersion' and we'll used the latest key.
            return 1;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            // At this point, we'll update the shell settings and release the shell.
            var shell = scope.ServiceProvider.GetRequiredService<ShellSettings>();
            var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();

            shell["RedisKeyVersion"] = "v1";

            await shellHost.UpdateShellSettingsAsync(shell);
            await shellHost.ReleaseShellContextAsync(shell);
        });

        return 1;
    }
}

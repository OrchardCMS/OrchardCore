using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Shells.Database.Configuration;

namespace OrchardCore.Shells.Database.Extensions;

public static class DatabaseShellContextFactoryExtensions
{
    internal static Task<ShellContext> GetDatabaseContextAsync(
        this IShellContextFactory shellContextFactory, DatabaseShellsStorageOptions options)
    {
        if (options.DatabaseProvider is null)
        {
            throw new InvalidOperationException("The 'OrchardCore.Shells.Database' configuration section should define a 'DatabaseProvider'");
        }

        var settings = new ShellSettings()
            .AsDefaultShell()
            .AsDisposable()
            .AsRunning();

        settings["DatabaseProvider"] = options.DatabaseProvider;
        settings["ConnectionString"] = options.ConnectionString;
        settings["TablePrefix"] = options.TablePrefix;
        settings["Schema"] = options.Schema;

        return shellContextFactory.CreateDescribedContextAsync(settings, new ShellDescriptor());
    }
}

using System;
using System.Threading.Tasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Shells.Database.Configuration;

namespace OrchardCore.Shells.Database.Extensions
{
    public static class DatabaseShellContextFactoryExtensions
    {
        internal static Task<ShellContext> GetDatabaseContextAsync(this IShellContextFactory shellContextFactory, DatabaseShellsStorageOptions options)
        {
            if (options.DatabaseProvider == null)
            {
                throw new ArgumentNullException(nameof(options.DatabaseProvider),
                    "The 'OrchardCore.Shells.Database' configuration section should define a 'DatabaseProvider'");
            }

            var settings = new ShellSettings
            {
                Name = ShellHelper.DefaultShellName,
                State = TenantState.Running,
                ["DatabaseProvider"] = options.DatabaseProvider,
                ["ConnectionString"] = options.ConnectionString,
                ["TablePrefix"] = options.TablePrefix,
                ["Schema"] = options.Schema
            };

            return shellContextFactory.CreateDescribedContextAsync(settings, new ShellDescriptor());
        }
    }
}

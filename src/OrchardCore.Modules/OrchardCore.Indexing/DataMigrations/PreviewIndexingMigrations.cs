using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using YesSql;

namespace OrchardCore.Indexing.DataMigrations;

// This migration can be removed before releasing Orchard Core 3.0.
internal sealed class PreviewIndexingMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public PreviewIndexingMigrations(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public int Create()
    {
        if (_shellSettings.IsInitializing())
        {
            return 1;
        }

        ShellScope.AddDeferredTask(async scope =>
        {
            var store = scope.ServiceProvider.GetRequiredService<IDocumentManager<DictionaryDocument<IndexProfile>>>();
            var document = await store.GetOrCreateImmutableAsync();

            if (document.Records.Count == 0)
            {
                // If there are no records, we can safely return.
                return;
            }

            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            foreach (var record in document.Records)
            {
                await session.SaveAsync(record.Value);
            }

            await session.FlushAsync();
        });

        return 1;
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using OrchardCore.ContentLocalization.Models;
using System.Threading;
using OrchardCore.ContentManagement;
using System.Threading;
using OrchardCore.ContentManagement.Metadata;
using System.Threading;
using OrchardCore.ContentManagement.Metadata.Settings;
using System.Threading;
using OrchardCore.Data.Migration;
using System.Threading;
using OrchardCore.Environment.Shell.Scope;
using System.Threading;
using YesSql;
using System.Threading;
using YesSql.Sql;
using System.Threading;

namespace OrchardCore.ContentLocalization.Records;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(LocalizationPart), builder => builder
            .Attachable()
            .WithDescription("Provides a way to create localized version of content."));

        await SchemaBuilder.CreateMapIndexTableAsync<LocalizedContentItemIndex>(table => table
            .Column<string>("LocalizationSet", col => col.WithLength(26))
            .Column<string>("Culture", col => col.WithLength(16))
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<bool>("Published")
            .Column<bool>("Latest")
        );

        await SchemaBuilder.AlterIndexTableAsync<LocalizedContentItemIndex>(table => table
            .CreateIndex("IDX_LocalizationPartIndex_DocumentId",
            "DocumentId",
            "LocalizationSet",
            "Culture",
            "ContentItemId",
            "Published",
            "Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<LocalizedContentItemIndex>(table => table
            .AddColumn<bool>(nameof(LocalizedContentItemIndex.Published)));

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<LocalizedContentItemIndex>(table => table
            .AddColumn<bool>(nameof(LocalizedContentItemIndex.Latest))
        );

        await SchemaBuilder.AlterIndexTableAsync<LocalizedContentItemIndex>(table => table
            .CreateIndex("IDX_LocalizationPartIndex_DocumentId",
            "DocumentId",
            "LocalizationSet",
            "Culture",
            "ContentItemId",
            "Published",
            "Latest")
        );

        return 3;
    }

    // Migrate null LocalizedContentItemIndex Latest column.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom3()
#pragma warning restore CA1822 // Mark members as static
    {
        // Defer this until after the subsequent migrations have succeeded as the schema has changed.
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var localizedContentItems = await session.Query<ContentItem, LocalizedContentItemIndex>().ListAsync(cancellationToken: CancellationToken.None);

            foreach (var localizedContentItem in localizedContentItems)
            {
                localizedContentItem.Latest = localizedContentItem.ContentItem.Latest;
                await session.SaveAsync(localizedContentItem, cancellationToken: CancellationToken.None);
            }
        });

        return 4;
    }
}

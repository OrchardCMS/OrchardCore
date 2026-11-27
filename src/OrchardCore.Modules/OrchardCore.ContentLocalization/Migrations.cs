using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Indexes;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using YesSql;
using YesSql.Sql;

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

        await CreateLocalizationSetIndexAsync();

        // Shortcut other migration steps on new content definition schemas.
        return 5;
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
    public static int UpdateFrom3()
    {
        // Defer this until after the subsequent migrations have succeeded as the schema has changed.
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var localizedContentItems = await session.Query<ContentItem, LocalizedContentItemIndex>().ListAsync();

            foreach (var localizedContentItem in localizedContentItems)
            {
                localizedContentItem.Latest = localizedContentItem.ContentItem.Latest;
                await session.SaveAsync(localizedContentItem);
            }
        });

        return 4;
    }

    /// <summary>
    /// Adds version-specific membership without changing the active-version localization index.
    /// </summary>
    public async Task<int> UpdateFrom4Async()
    {
        await CreateLocalizationSetIndexAsync();
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var lastId = 0L;
            while (true)
            {
                var items = await session.Query<ContentItem, ContentItemIndex>(index => index.DocumentId > lastId)
                    .OrderBy(index => index.DocumentId).Take(100).ListAsync();
                if (items.Count == 0)
                {
                    break;
                }

                foreach (var item in items)
                {
                    if (item.Has<LocalizationPart>())
                    {
                        await session.SaveAsync(item);
                    }

                    lastId = item.Id;
                }

                await session.FlushAsync();
            }
        });
        return 5;
    }

    private async Task CreateLocalizationSetIndexAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<LocalizationSetIndex>(table => table
            .Column<string>(nameof(LocalizationSetIndex.LocalizationSet), column => column.WithLength(26)));
        await SchemaBuilder.AlterIndexTableAsync<LocalizationSetIndex>(table => table
            .CreateIndex("IDX_LocalizationSetIndex_DocumentId", "DocumentId", nameof(LocalizationSetIndex.LocalizationSet)));
    }
}

using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("FlowPart", builder => builder
            .Attachable()
            .WithDescription("Provides a customizable body for your content item where you can build a content structure with widgets."));

        await _contentDefinitionManager.AlterPartDefinitionAsync("BagPart", builder => builder
            .Attachable()
            .Reusable()
            .WithDescription("Provides a collection behavior for your content item where you can place other content items."));

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("BagPart", builder => builder
            .Attachable()
            .Reusable()
            .WithDescription("Provides a collection behavior for your content item where you can place other content items."));

        return 2;
    }

    // Migrate PartSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager.MigratePartSettingsAsync<BagPart, BagPartSettings>();

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        // In Version 3, we introduced the 'CollapseContainedItems' setting for 'BagPartSettings' and 'FlowPartSettings'.
        // This allows users to control whether contained items are collapsed or expanded by default.
        // To maintain the behavior for existing tenants, this migration sets 'CollapseContainedItems' to true for 'BagPart'.
        // For consistency, both 'BagPart' and 'FlowPart' will expand contained items when the page loads by default.

        await _contentDefinitionManager.AlterPartDefinitionAsync("BagPart", part => part
            .MergeSettings<BagPartSettings>(settings =>
            {
                settings.CollapseContainedItems = true;
            }));

        var typeDefinitions = await _contentDefinitionManager.LoadTypeDefinitionsAsync();

        foreach (var typeDefinition in typeDefinitions)
        {
            ContentTypeDefinitionBuilder builder = null;

            foreach (var partDefinition in typeDefinition.Parts)
            {
                if (partDefinition.PartDefinition.Name != "BagPart")
                {
                    continue;
                }

                var name = partDefinition.IsNamedPart()
                    ? partDefinition.Name
                    : partDefinition.PartDefinition.Name;

                builder ??= new ContentTypeDefinitionBuilder(typeDefinition);
                builder
                    .WithPart<BagPart>(name, part => part
                        .MergeSettings<BagPartSettings>(settings =>
                        {
                            settings.CollapseContainedItems = true;
                        })
                    );
            }

            if (builder is not null)
            {
                await _contentDefinitionManager.StoreTypeDefinitionAsync(builder.Build());

                builder = null;
            }
        }

        return 4;
    }
}

using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Autoroute;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("AutoroutePart", builder => builder
            .Attachable()
            .WithDescription("Provides a custom url for your content item."));

        await SchemaBuilder.CreateMapIndexTableAsync<AutoroutePartIndex>(table => table
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<string>("ContainedContentItemId", c => c.WithLength(26))
            .Column<string>("JsonPath", c => c.Unlimited())
            .Column<string>("Path", col => col.WithLength(AutoroutePart.MaxPathLength))
            .Column<bool>("Published")
            .Column<bool>("Latest")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 5;
    }

    // Migrate PartSettings. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.MigratePartSettingsAsync<AutoroutePart, AutoroutePartSettings>();

        return 2;
    }

    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom2()
#pragma warning restore CA1822 // Mark members as static
    {
        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<AutoroutePartIndex>(table => table
            .AddColumn<string>("ContainedContentItemId", c => c.WithLength(26))
        );

        await SchemaBuilder.AlterIndexTableAsync<AutoroutePartIndex>(table => table
            .AddColumn<string>("JsonPath", c => c.Unlimited())
        );

        await SchemaBuilder.AlterIndexTableAsync<AutoroutePartIndex>(table => table
            .AddColumn<bool>("Latest", c => c.WithDefault(false))
        );

        return 4;
    }

    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom4()
#pragma warning restore CA1822 // Mark members as static
    {
        return 5;
    }
}

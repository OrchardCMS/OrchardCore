using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Search.Migrations;

public sealed class SearchMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SearchMigrations(IContentDefinitionManager contentDefinitionManager)
        => _contentDefinitionManager = contentDefinitionManager;

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("SearchFormPart", part => part
            .WithDisplayName("Search Form Part")
            .Attachable()
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("SearchForm", type => type
            .Stereotype("Widget")
            .DisplayedAs("Search Form")
            .WithDescription("Provides a search form")
            .WithPart("SearchFormPart")
        );

        return 1;
    }
}

using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Search.Migrations;

public class SearchMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SearchMigrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public int Create()
    {
        _contentDefinitionManager.AlterPartDefinition("SearchPart", part => part
            .WithDisplayName("Search Part")
        );

        _contentDefinitionManager.AlterTypeDefinition("SearchWidget", type => type
            .Stereotype("Widget")
            .DisplayedAs("Search")
            .WithDescription("Provides a search form")
            .WithPart("SearchPart")
        );

        return 1;
    }
}

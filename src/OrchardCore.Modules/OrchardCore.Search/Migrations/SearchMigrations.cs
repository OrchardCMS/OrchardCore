using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Search.Migrations;

public class SearchMigrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public SearchMigrations(IContentDefinitionManager contentDefinitionManager)
        => _contentDefinitionManager = contentDefinitionManager;

    public int Create()
    {
        _contentDefinitionManager.AlterPartDefinition("SearchFormPart", part => part
            .WithDisplayName("Search Form Part")
            .Attachable()
        );

        _contentDefinitionManager.AlterTypeDefinition("SearchForm", type => type
            .Stereotype("Widget")
            .DisplayedAs("Search Form")
            .WithDescription("Provides a search form")
            .WithPart("SearchFormPart")
        );

        return 1;
    }
}

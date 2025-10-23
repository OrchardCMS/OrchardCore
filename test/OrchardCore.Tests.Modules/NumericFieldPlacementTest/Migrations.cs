using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace NumericFieldPlacementTest;

public class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("FieldsAccordionPart", part => part
            .WithField("MaintenanceFeesFinance", field => field
                .OfType(nameof(NumericField))
                .WithDisplayName("Maintenance Fees Finance")
                .WithEditor("Spinner")  // This is the key - when an editor is set, the shape type changes
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "Enter the maintenance fees",
                    Required = false,
                    Scale = 2,
                    Minimum = 0,
                })
            )
            .WithField("TestNumericNoEditor", field => field
                .OfType(nameof(NumericField))
                .WithDisplayName("Test Numeric No Editor")
                .WithSettings(new NumericFieldSettings
                {
                    Hint = "This field has no editor specified",
                    Required = false,
                })
            )
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("TestContentType", type => type
            .WithPart("FieldsAccordionPart")
            .Creatable()
            .Listable()
        );

        return 1;
    }
}

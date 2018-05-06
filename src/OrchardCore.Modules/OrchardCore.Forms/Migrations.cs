using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            // Form
            _contentDefinitionManager.AlterPartDefinition("FormPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a form."));

            _contentDefinitionManager.AlterTypeDefinition("Form", type => type
                .WithPart("FormPart")
                .Stereotype("Widget"));

            // Input
            _contentDefinitionManager.AlterPartDefinition("InputPart", part => part
                .WithDescription("Provides input field properties."));

            _contentDefinitionManager.AlterTypeDefinition("Input", type => type
                .WithPart("InputPart")
                .Stereotype("Widget"));

            // TextArea
            _contentDefinitionManager.AlterPartDefinition("TextAreaPart", part => part
                .WithDescription("Provides text area properties."));

            _contentDefinitionManager.AlterTypeDefinition("TextArea", type => type
                .WithPart("TextAreaPart")
                .Stereotype("Widget"));

            // Select
            _contentDefinitionManager.AlterPartDefinition("SelectPart", part => part
                .WithDescription("Provides select field properties."));

            _contentDefinitionManager.AlterTypeDefinition("Select", type => type
                .WithPart("SelectPart")
                .Stereotype("Widget"));

            // Button
            _contentDefinitionManager.AlterPartDefinition("ButtonPart", part => part
                .WithDescription("Provides button properties."));

            _contentDefinitionManager.AlterTypeDefinition("Button", type => type
                .WithPart("ButtonPart")
                .Stereotype("Widget"));

            return 1;
        }
    }
}
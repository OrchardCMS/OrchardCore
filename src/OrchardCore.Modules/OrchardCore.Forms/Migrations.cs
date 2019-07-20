using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Forms
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<int> CreateAsync()
        {
            // Form
            await _contentDefinitionManager.AlterPartDefinitionAsync("FormPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a form."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Form", type => type
                .WithPart("TitlePart")
                .WithPart("FormPart")
                .WithPart("FlowPart")
                .Stereotype("Widget"));

            // FormElement
            await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementPart", part => part
                .WithDescription("Provides attributes common to all form elements."));

            // FormInputElement
            await _contentDefinitionManager.AlterPartDefinitionAsync("FormInputElementPart", part => part
                .WithDescription("Provides attributes common to all input form elements."));

            // Label
            await _contentDefinitionManager.AlterPartDefinitionAsync("LabelPart", part => part
                .WithDescription("Provides label properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Label", type => type
                .WithPart("TitlePart")
                .WithPart("FormElementPart")
                .WithPart("LabelPart")
                .Stereotype("Widget"));

            // Input
            await _contentDefinitionManager.AlterPartDefinitionAsync("InputPart", part => part
                .WithDescription("Provides input field properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Input", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("InputPart")
                .Stereotype("Widget"));

            // TextArea
            await _contentDefinitionManager.AlterPartDefinitionAsync("TextAreaPart", part => part
                .WithDescription("Provides text area properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("TextArea", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("TextAreaPart")
                .Stereotype("Widget"));

            // Select
            await _contentDefinitionManager.AlterPartDefinitionAsync("SelectPart", part => part
                .WithDescription("Provides select field properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Select", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("SelectPart")
                .Stereotype("Widget"));

            // Button
            await _contentDefinitionManager.AlterPartDefinitionAsync("ButtonPart", part => part
                .WithDescription("Provides button properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Button", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("ButtonPart")
                .Stereotype("Widget"));

            // Validation Summary
            await _contentDefinitionManager.AlterPartDefinitionAsync("ValidationSummaryPart", part => part
                .WithDescription("Displays a validation summary."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("ValidationSummary", type => type
                .WithPart("ValidationSummaryPart")
                .Stereotype("Widget"));

            // Validation
            await _contentDefinitionManager.AlterPartDefinitionAsync("ValidationPart", part => part
                .WithDescription("Displays a field validation error."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("Validation", type => type
                .WithPart("ValidationPart")
                .Stereotype("Widget"));

            return 1;
        }
    }
}

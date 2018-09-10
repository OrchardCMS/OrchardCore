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

        public int Create()
        {
            // Form
            _contentDefinitionManager.AlterPartDefinition("FormPart", part => part
                .Attachable()
                .WithDescription("Turns your content item into a form."));

            _contentDefinitionManager.AlterTypeDefinition("Form", type => type
                .WithPart("TitlePart")
                .WithPart("FormPart")
                .WithPart("FlowPart")
                .Stereotype("Widget"));

            // FormElement
            _contentDefinitionManager.AlterPartDefinition("FormElementPart", part => part
                .WithDescription("Provides attributes common to all form elements."));

            // FormInputElement
            _contentDefinitionManager.AlterPartDefinition("FormInputElementPart", part => part
                .WithDescription("Provides attributes common to all input form elements."));

            // Label
            _contentDefinitionManager.AlterPartDefinition("LabelPart", part => part
                .WithDescription("Provides label properties."));

            _contentDefinitionManager.AlterTypeDefinition("Label", type => type
                .WithPart("TitlePart")
                .WithPart("FormElementPart")
                .WithPart("LabelPart")
                .Stereotype("Widget"));

            // Input
            _contentDefinitionManager.AlterPartDefinition("InputPart", part => part
                .WithDescription("Provides input field properties."));

            _contentDefinitionManager.AlterTypeDefinition("Input", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("InputPart")
                .Stereotype("Widget"));

            // TextArea
            _contentDefinitionManager.AlterPartDefinition("TextAreaPart", part => part
                .WithDescription("Provides text area properties."));

            _contentDefinitionManager.AlterTypeDefinition("TextArea", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("TextAreaPart")
                .Stereotype("Widget"));

            // Select
            _contentDefinitionManager.AlterPartDefinition("SelectPart", part => part
                .WithDescription("Provides select field properties."));

            _contentDefinitionManager.AlterTypeDefinition("Select", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("SelectPart")
                .Stereotype("Widget"));

            // Button
            _contentDefinitionManager.AlterPartDefinition("ButtonPart", part => part
                .WithDescription("Provides button properties."));

            _contentDefinitionManager.AlterTypeDefinition("Button", type => type
                .WithPart("FormInputElementPart")
                .WithPart("FormElementPart")
                .WithPart("ButtonPart")
                .Stereotype("Widget"));

            // NoCaptcha
            _contentDefinitionManager.AlterPartDefinition("NoCaptchaPart", part => part
                .WithDescription("Provides captcha properties."));

            _contentDefinitionManager.AlterTypeDefinition("NoCaptcha", type => type
                .WithPart("NoCaptchaPart")
                .Stereotype("Widget"));

            // Validation Summary
            _contentDefinitionManager.AlterPartDefinition("ValidationSummaryPart", part => part
                .WithDescription("Displays a validation summary."));

            _contentDefinitionManager.AlterTypeDefinition("ValidationSummary", type => type
                .WithPart("ValidationSummaryPart")
                .Stereotype("Widget"));

            // Validation
            _contentDefinitionManager.AlterPartDefinition("ValidationPart", part => part
                .WithDescription("Displays a field validation error."));

            _contentDefinitionManager.AlterTypeDefinition("Validation", type => type
                .WithPart("ValidationPart")
                .Stereotype("Widget"));

            return 1;
        }
    }
}
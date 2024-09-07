using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Forms;

public sealed class Migrations : DataMigration
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
            .WithPart("TitlePart", part => part
                .WithSettings(new TitlePartSettings { RenderTitle = false })
                .WithPosition("0")
            )
            .WithPart("FormElementPart", part =>
               part.WithPosition("1")
            )
            .WithPart("FormPart")
            .WithPart("FlowPart")
            .Stereotype("Widget"));

        // FormElement
        await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementPart", part => part
            .WithDescription("Provides attributes common to all form elements."));

        await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementLabelPart", part => part
            .Attachable()
            .WithDescription("Provides a way to capture element's label.")
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementValidationPart", part => part
            .Attachable()
            .WithDescription("Provides validation options to form elements.")
        );

        // FormInputElement
        await _contentDefinitionManager.AlterPartDefinitionAsync("FormInputElementPart", part => part
            .WithDescription("Provides attributes common to all input form elements."));

        // Label
        await _contentDefinitionManager.AlterPartDefinitionAsync("LabelPart", part => part
            .WithDescription("Provides label properties."));

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Label", type => type
            .WithPart("TitlePart", part => part
                .WithSettings(new TitlePartSettings { RenderTitle = false })
            )
            .WithPart("FormElementPart")
            .WithPart("LabelPart")
            .Stereotype("Widget"));

        // Input
        await _contentDefinitionManager.AlterPartDefinitionAsync("InputPart", part => part
            .WithDescription("Provides input field properties."));

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Input", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("InputPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
            .Stereotype("Widget"));

        // TextArea
        await _contentDefinitionManager.AlterPartDefinitionAsync("TextAreaPart", part => part
            .WithDescription("Provides text area properties."));

        await _contentDefinitionManager.AlterTypeDefinitionAsync("TextArea", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("TextAreaPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
            .Stereotype("Widget"));

        // Select
        await _contentDefinitionManager.AlterPartDefinitionAsync("SelectPart", part => part
            .WithDescription("Provides select field properties."));

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Select", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("SelectPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
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

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Form", type => type
            .WithPart("TitlePart", part => part.MergeSettings<TitlePartSettings>(setting => setting.RenderTitle = false))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Label", type => type
            .WithPart("TitlePart", part => part.MergeSettings<TitlePartSettings>(setting => setting.RenderTitle = false))
        );

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync("Form", type => type
            .WithPart("TitlePart", part => part
                .WithPosition("0")
            )
            .WithPart("FormElementPart", part =>
               part.WithPosition("1")
            )
        );

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementLabelPart", part => part
            .Attachable()
            .WithDescription("Provides a way to capture element's label.")
        );

        await _contentDefinitionManager.AlterPartDefinitionAsync("FormElementValidationPart", part => part
            .Attachable()
            .WithDescription("Provides validation options to form elements.")
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Select", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("SelectPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Input", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("InputPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("TextArea", type => type
            .WithPart("FormInputElementPart", part => part
                .WithPosition("1")
            )
            .WithPart("FormElementPart", part => part
                .WithPosition("2")
            )
            .WithPart("FormElementLabelPart", part => part
                .WithPosition("3")
            )
            .WithPart("TextAreaPart", part => part
                .WithPosition("4")
            )
            .WithPart("FormElementValidationPart", part => part
                .WithPosition("5")
            )
        );

        return 4;
    }

    internal sealed class TitlePartSettings
    {
        public int Options { get; set; }

        public string Pattern { get; set; }

        [DefaultValue(true)]
        public bool RenderTitle { get; set; }
    }
}

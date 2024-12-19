using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TaxonomyFieldDisplayDriver : ContentFieldDisplayDriver<TaxonomyField>
{
    private readonly IContentManager _contentManager;

    internal readonly IStringLocalizer S;

    public TaxonomyFieldDisplayDriver(
        IContentManager contentManager,
        IStringLocalizer<TaxonomyFieldDisplayDriver> localizer)
    {
        _contentManager = contentManager;
        S = localizer;
    }

    public override IDisplayResult Display(TaxonomyField field, BuildFieldDisplayContext context)
    {
        return Initialize<DisplayTaxonomyFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        }).Location("Detail", "Content")
        .Location("Summary", "Content");
    }

    public override IDisplayResult Edit(TaxonomyField field, BuildFieldEditorContext context)
    {
        return Initialize<EditTaxonomyFieldViewModel>(GetEditorShapeType(context), async model =>
        {
            var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();
            model.Taxonomy = await _contentManager.GetAsync(settings.TaxonomyContentItemId, VersionOptions.Latest);

            if (model.Taxonomy != null)
            {
                var termEntries = new List<TermEntry>();
                TaxonomyFieldDriverHelper.PopulateTermEntries(termEntries, field, model.Taxonomy.As<TaxonomyPart>().Terms, 0);

                model.TermEntries = termEntries;
                model.UniqueValue = termEntries.FirstOrDefault(x => x.Selected)?.ContentItemId;
            }

            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(TaxonomyField field, UpdateFieldEditorContext context)
    {
        var model = new EditTaxonomyFieldViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var settings = context.PartFieldDefinition.GetSettings<TaxonomyFieldSettings>();

        field.TaxonomyContentItemId = settings.TaxonomyContentItemId;
        field.TermContentItemIds = model.TermEntries.Where(x => x.Selected).Select(x => x.ContentItemId).ToArray();

        if (settings.Unique && !string.IsNullOrEmpty(model.UniqueValue))
        {
            field.TermContentItemIds = [model.UniqueValue];
        }

        if (settings.Required && field.TermContentItemIds.Length == 0)
        {
            context.Updater.ModelState.AddModelError(
                nameof(EditTaxonomyFieldViewModel.TermEntries),
                S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
        }

        return Edit(field, context);
    }
}

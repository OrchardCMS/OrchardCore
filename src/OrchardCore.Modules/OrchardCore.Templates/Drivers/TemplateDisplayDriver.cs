using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Drivers;

/// <summary>
/// Builds the editor of a template, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class TemplateDisplayDriver : DisplayDriver<TemplateViewModel>
{
    internal readonly IStringLocalizer S;

    public TemplateDisplayDriver(IStringLocalizer<TemplateDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(TemplateViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<TemplateViewModel>("TemplateGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<TemplateViewModel>("TemplateContent_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Template"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(TemplateViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Description, m => m.Content);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(TemplateViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(TemplateViewModel source, TemplateViewModel model)
    {
        model.AdminTemplates = source.AdminTemplates;
        model.Name = source.Name;
        model.Description = source.Description;
        model.Content = source.Content;
    }
}

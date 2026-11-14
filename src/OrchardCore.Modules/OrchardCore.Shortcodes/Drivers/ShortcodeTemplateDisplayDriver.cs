using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Shortcodes.ViewModels;

namespace OrchardCore.Shortcodes.Drivers;

/// <summary>
/// Builds the editor of a shortcode template, whose fields are grouped in cards by their placement.
/// The cards placed in the "Content" zone are shown beside the preview of the shortcode, the ones placed in the "Footer" zone below it.
/// </summary>
internal sealed class ShortcodeTemplateDisplayDriver : DisplayDriver<ShortcodeTemplateViewModel>
{
    internal readonly IStringLocalizer S;

    public ShortcodeTemplateDisplayDriver(IStringLocalizer<ShortcodeTemplateDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ShortcodeTemplateViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<ShortcodeTemplateViewModel>("ShortcodeTemplateGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            // The page creating a template describes its content differently.
            Initialize<ShortcodeTemplateViewModel>(context.IsNew ? "ShortcodeTemplateCreateContent_Edit" : "ShortcodeTemplateContent_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Footer", "1").Card(S["Template"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(ShortcodeTemplateViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Hint,
            m => m.Usage,
            m => m.SelectedCategories,
            m => m.DefaultValue,
            m => m.Content);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(ShortcodeTemplateViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(ShortcodeTemplateViewModel source, ShortcodeTemplateViewModel model)
    {
        model.Name = source.Name;
        model.Content = source.Content;
        model.Hint = source.Hint;
        model.Usage = source.Usage;
        model.DefaultValue = source.DefaultValue;
        model.Categories = source.Categories;
        model.SelectedCategories = source.SelectedCategories;
    }
}

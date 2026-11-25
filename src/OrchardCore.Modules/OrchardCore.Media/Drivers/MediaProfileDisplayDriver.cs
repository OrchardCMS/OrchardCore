using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Drivers;

/// <summary>
/// Builds the editor of a media profile, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class MediaProfileDisplayDriver : DisplayDriver<MediaProfileViewModel>
{
    internal readonly IStringLocalizer S;

    public MediaProfileDisplayDriver(IStringLocalizer<MediaProfileDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(MediaProfileViewModel model, BuildEditorContext context)
    {
        // The pages creating and editing a media profile lay out their fields differently.
        var shapePrefix = context.IsNew ? "MediaProfileCreate" : "MediaProfile";

        return Combine(
            Initialize<MediaProfileViewModel>($"{shapePrefix}General_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<MediaProfileViewModel>($"{shapePrefix}ImageProcessing_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Image processing"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(MediaProfileViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Hint,
            m => m.SelectedWidth,
            m => m.CustomWidth,
            m => m.SelectedHeight,
            m => m.CustomHeight,
            m => m.SelectedMode,
            m => m.SelectedFormat,
            m => m.Quality,
            m => m.BackgroundColor,
            m => m.AutoOrient);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(MediaProfileViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(MediaProfileViewModel source, MediaProfileViewModel model)
    {
        model.Name = source.Name;
        model.Hint = source.Hint;
        model.SelectedWidth = source.SelectedWidth;
        model.CustomWidth = source.CustomWidth;
        model.SelectedHeight = source.SelectedHeight;
        model.CustomHeight = source.CustomHeight;
        model.SelectedMode = source.SelectedMode;
        model.SelectedFormat = source.SelectedFormat;
        model.Quality = source.Quality;
        model.BackgroundColor = source.BackgroundColor;
        model.AutoOrient = source.AutoOrient;
        model.AvailableWidths = source.AvailableWidths;
        model.AvailableHeights = source.AvailableHeights;
        model.AvailableResizeModes = source.AvailableResizeModes;
        model.AvailableFormats = source.AvailableFormats;
    }
}

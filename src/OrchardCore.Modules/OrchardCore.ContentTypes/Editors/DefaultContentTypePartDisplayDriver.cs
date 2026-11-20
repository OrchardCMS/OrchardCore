using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Renders the common settings of a part of a content type, the display name and description of a named part, its
/// editor and display mode, in the "General" card of the type part editor.
/// </summary>
/// <remarks>
/// The shape is placed in the <c>Header</c> zone, which the page renders before the settings of the selected editor
/// and the other settings of the part. The controller binds and saves these settings.
/// </remarks>
internal sealed class DefaultContentTypePartDisplayDriver : ContentTypePartDefinitionDisplayDriver
{
    private readonly IThemeManager _themeManager;
    private readonly IShapeTableManager _shapeTableManager;

    internal readonly IStringLocalizer S;

    public DefaultContentTypePartDisplayDriver(
        IThemeManager themeManager,
        IShapeTableManager shapeTableManager,
        IStringLocalizer<DefaultContentTypePartDisplayDriver> stringLocalizer)
    {
        _themeManager = themeManager;
        _shapeTableManager = shapeTableManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> EditAsync(ContentTypePartDefinition model, BuildEditorContext context)
    {
        return BuildAsync(model, new EditTypePartViewModel
        {
            DisplayName = model.DisplayName(),
            Description = model.Description(),
            Editor = model.Editor(),
            DisplayMode = model.DisplayMode(),
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition model, UpdateTypePartEditorContext context)
    {
        // The posted values are displayed again when the page is redisplayed, e.g. the selected editor.
        var viewModel = new EditTypePartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, m => m.DisplayName, m => m.Description, m => m.Editor, m => m.DisplayMode);

        return await BuildAsync(model, viewModel);
    }

    // The fields keep the names the page posts, which the controller binds and validates.
    protected override void BuildPrefix(ContentTypePartDefinition model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private async Task<IDisplayResult> BuildAsync(ContentTypePartDefinition typePartDefinition, EditTypePartViewModel values)
    {
        var shapeTable = await _themeManager.GetShapeTableAsync(_shapeTableManager);
        var partName = typePartDefinition.PartDefinition.Name;
        var editorShapes = ShapeOptionsHelper.GetOptionShapes(shapeTable, partName + "_Option");
        var displayShapes = ShapeOptionsHelper.GetOptionShapes(shapeTable, partName + "_DisplayOption");

        // The card is only rendered when one of its settings applies to the part.
        if (!typePartDefinition.PartDefinition.IsReusable() && editorShapes.Length == 0 && displayShapes.Length == 0)
        {
            return null;
        }

        return Initialize<EditTypePartViewModel>("ContentTypePartGeneral_Edit", model =>
        {
            model.Name = typePartDefinition.Name;
            model.DisplayName = values.DisplayName;
            model.Description = values.Description;
            model.Editor = values.Editor;
            model.DisplayMode = values.DisplayMode;
            model.TypePartDefinition = typePartDefinition;
            model.EditorShapes = editorShapes;
            model.DisplayShapes = displayShapes;
        }).Location(l => l.Zone("Header", "1").Card(S["General"], "10"));
    }
}

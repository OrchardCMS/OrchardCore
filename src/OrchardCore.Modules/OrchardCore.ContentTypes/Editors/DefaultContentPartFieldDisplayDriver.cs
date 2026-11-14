using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Renders the common settings of a field, its display name, technical name, editor and display mode, in the
/// "General" card of the field editor.
/// </summary>
/// <remarks>
/// The shape is placed in the <c>Header</c> zone, which the page renders before the settings of the selected editor
/// and the other settings of the field. The controller binds and saves these settings.
/// </remarks>
internal sealed class DefaultContentPartFieldDisplayDriver : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IThemeManager _themeManager;
    private readonly IShapeTableManager _shapeTableManager;

    internal readonly IStringLocalizer S;

    public DefaultContentPartFieldDisplayDriver(
        IThemeManager themeManager,
        IShapeTableManager shapeTableManager,
        IStringLocalizer<DefaultContentPartFieldDisplayDriver> stringLocalizer)
    {
        _themeManager = themeManager;
        _shapeTableManager = shapeTableManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> EditAsync(ContentPartFieldDefinition model, BuildEditorContext context)
    {
        return BuildAsync(model, model.DisplayName(), model.Editor(), model.DisplayMode());
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition model, UpdatePartFieldEditorContext context)
    {
        // The posted values are displayed again when the page is redisplayed, e.g. the selected editor.
        var viewModel = new EditFieldViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, m => m.DisplayName, m => m.Editor, m => m.DisplayMode);

        return await BuildAsync(model, viewModel.DisplayName, viewModel.Editor, viewModel.DisplayMode);
    }

    // The fields keep the names the page posts, which the controller binds and validates.
    protected override void BuildPrefix(ContentPartFieldDefinition model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private async Task<IDisplayResult> BuildAsync(ContentPartFieldDefinition partFieldDefinition, string displayName, string editor, string displayMode)
    {
        var shapeTable = await _themeManager.GetShapeTableAsync(_shapeTableManager);
        var fieldName = partFieldDefinition.FieldDefinition.Name;

        return Initialize<EditFieldViewModel>("ContentPartFieldGeneral_Edit", model =>
        {
            model.Name = partFieldDefinition.Name;
            model.DisplayName = displayName;
            model.Editor = editor;
            model.DisplayMode = displayMode;
            model.PartFieldDefinition = partFieldDefinition;
            model.EditorShapes = ShapeOptionsHelper.GetOptionShapes(shapeTable, fieldName + "_Option");
            model.DisplayShapes = ShapeOptionsHelper.GetOptionShapes(shapeTable, fieldName + "_DisplayOption");
        }).Location(l => l.Zone("Header", "1").Card(S["General"], "10"));
    }
}

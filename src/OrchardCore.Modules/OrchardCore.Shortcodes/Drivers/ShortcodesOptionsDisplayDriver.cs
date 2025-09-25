using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;

namespace OrchardCore.Shortcodes.Drivers;

public sealed class ShortcodeFilterDisplayDriver : DisplayDriver<ShortcodeFilter>
{
    // Maintain the Options prefix for compatibility with binding.
    protected override void BuildPrefix(ShortcodeFilter model, string htmlFieldPrefix)
    {
        Prefix = nameof(ShortcodeTemplateIndexViewModel.Filter);
    }

    public override Task<IDisplayResult> DisplayAsync(
        ShortcodeFilter model,
        BuildDisplayContext context
    )
    {
        return CombineAsync(
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminListBulkActions",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("BulkActions", "Content:10"),
            View("ShortcodesAdminFilters_Thumbnail__DisplayText", model)
                .Location("Thumbnail", "Content:10"),
            View("ShortcodesAdminFilters_Thumbnail__Sort", model)
                .Location("Thumbnail", "Content:50")
        );
    }

    public override Task<IDisplayResult> EditAsync(
        ShortcodeFilter model,
        BuildEditorContext context
    )
    {
        // Map the filter result to a model so the ui can reflect current selections.
        model.FilterResult.MapTo(model);

        return CombineAsync(
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminListSearch",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("Search:10"),
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminListCreate",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("Create:10"),
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminListSummary",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("Summary:10"),
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminListFilters",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("Actions:10.1"),
            Initialize<ShortcodeFilter>(
                    "ShortcodesAdminList_Fields_BulkActions",
                    m => BuildFilterViewModel(m, model)
                )
                .Location("Actions:10.1")
        );
    }

    public override Task<IDisplayResult> UpdateAsync(
        ShortcodeFilter model,
        UpdateEditorContext context
    )
    {
        // Map the incoming values from a form post to the filter result.
        model.FilterResult.MapFrom(model);

        return EditAsync(model, context);
    }

    private static void BuildFilterViewModel(ShortcodeFilter m, ShortcodeFilter model)
    {
        m.SearchText = model.SearchText;
        m.OriginalSearchText = model.OriginalSearchText;
        m.Name = model.Name;
        m.FilterResult = model.FilterResult;
        m.AllItems = model.AllItems;
        m.StartIndex = model.StartIndex;
        m.EndIndex = model.EndIndex;
        m.ShortcodeItemsCount = model.ShortcodeItemsCount;
        m.TotalItemCount = model.TotalItemCount;
        m.RouteValues = model.RouteValues;
        m.ContentsBulkAction = model.ContentsBulkAction;
        m.ContentSorts = model.ContentSorts;
        m.OrderBy = model.OrderBy;
        m.BulkAction = model.BulkAction;
    }
}

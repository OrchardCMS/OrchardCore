using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers;

public sealed class ContentOptionsDisplayDriver : DisplayDriver<ContentOptionsViewModel>
{
    // Maintain the Options prefix for compatibility with binding.
    protected override void BuildPrefix(ContentOptionsViewModel model, string htmlFieldPrefix)
    {
        Prefix = "Options";
    }

    public override Task<IDisplayResult> DisplayAsync(ContentOptionsViewModel model, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<ContentOptionsViewModel>("ContentsAdminListBulkActions", m => BuildContentOptionsViewModel(m, model))
            .Location("BulkActions", "Content:10"),
            View("ContentsAdminFilters_Thumbnail__DisplayText", model).Location("Thumbnail", "Content:10"),
            View("ContentsAdminFilters_Thumbnail__ContentType", model).Location("Thumbnail", "Content:20"),
            View("ContentsAdminFilters_Thumbnail__Stereotype", model).Location("Thumbnail", "Content:30"),
            View("ContentsAdminFilters_Thumbnail__Status", model).Location("Thumbnail", "Content:40"),
            View("ContentsAdminFilters_Thumbnail__Sort", model).Location("Thumbnail", "Content:50")
        );
    }

    public override Task<IDisplayResult> EditAsync(ContentOptionsViewModel model, BuildEditorContext context)
    {
        // Map the filter result to a model so the ui can reflect current selections.
        model.FilterResult.MapTo(model);

        return CombineAsync(
            Initialize<ContentOptionsViewModel>("ContentsAdminListSearch", m => BuildContentOptionsViewModel(m, model)).Location("Search:10"),
            Initialize<ContentOptionsViewModel>("ContentsAdminListCreate", m => BuildContentOptionsViewModel(m, model)).Location("Create:10"),
            Initialize<ContentOptionsViewModel>("ContentsAdminListSummary", m => BuildContentOptionsViewModel(m, model)).Location("Summary:10"),
            Initialize<ContentOptionsViewModel>("ContentsAdminListFilters", m => BuildContentOptionsViewModel(m, model)).Location("Actions:10.1"),
            Initialize<ContentOptionsViewModel>("ContentsAdminList_Fields_BulkActions", m => BuildContentOptionsViewModel(m, model)).Location("Actions:10.1")
        );
    }

    public override Task<IDisplayResult> UpdateAsync(ContentOptionsViewModel model, UpdateEditorContext context)
    {
        // Map the incoming values from a form post to the filter result.
        model.FilterResult.MapFrom(model);

        return EditAsync(model, context);
    }

    private static void BuildContentOptionsViewModel(ContentOptionsViewModel m, ContentOptionsViewModel model)
    {
        m.ContentTypeOptions = model.ContentTypeOptions;
        m.ContentStatuses = model.ContentStatuses;
        m.ContentSorts = model.ContentSorts;
        m.ContentsBulkAction = model.ContentsBulkAction;
        m.CreatableTypes = model.CreatableTypes;
        m.StartIndex = model.StartIndex;
        m.EndIndex = model.EndIndex;
        m.ContentItemsCount = model.ContentItemsCount;
        m.TotalItemCount = model.TotalItemCount;
        m.SearchText = model.SearchText;
        m.OriginalSearchText = model.OriginalSearchText;
        m.ContentsStatus = model.ContentsStatus;
        m.OrderBy = model.OrderBy;
        m.SelectedContentType = model.SelectedContentType;
        m.FilterResult = model.FilterResult;
    }
}

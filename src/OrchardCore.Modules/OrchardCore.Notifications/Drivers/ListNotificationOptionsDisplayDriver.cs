using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.Drivers;

public sealed class ListNotificationOptionsDisplayDriver : DisplayDriver<ListNotificationOptions>
{
    // Maintain the Options prefix for compatibility with binding.
    protected override void BuildPrefix(ListNotificationOptions model, string htmlFieldPrefix)
    {
        Prefix = "Options";
    }

    public override Task<IDisplayResult> DisplayAsync(ListNotificationOptions model, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<ListNotificationOptions>("NotificationsAdminListBulkActions", m => BuildOptionsViewModel(m, model))
                .Location("BulkActions", "Content:10"),
            View("NotificationsAdminFilters_Thumbnail__Status", model)
                .Location("Thumbnail", "Content:30"),
            View("NotificationsAdminFilters_Thumbnail__Sort", model)
                .Location("Thumbnail", "Content:40")
        );
    }

    public override Task<IDisplayResult> EditAsync(ListNotificationOptions model, BuildEditorContext context)
    {
        model.FilterResult.MapTo(model);

        return CombineAsync(
            Initialize<ListNotificationOptions>("NotificationsAdminListBulkActions", m => BuildOptionsViewModel(m, model))
                .Location("BulkActions", "Content:10"),
            Initialize<ListNotificationOptions>("NotificationsAdminListSearch", m => BuildOptionsViewModel(m, model))
                .Location("Search:10"),
            Initialize<ListNotificationOptions>("NotificationsAdminListActionBarButtons", m => BuildOptionsViewModel(m, model))
                .Location("ActionBarButtons:10"),
            Initialize<ListNotificationOptions>("NotificationsAdminListSummary", m => BuildOptionsViewModel(m, model))
                .Location("Summary:10"),
            Initialize<ListNotificationOptions>("NotificationsAdminListFilters", m => BuildOptionsViewModel(m, model))
                .Location("Actions:10.1"),
            Initialize<ListNotificationOptions>("NotificationsAdminList_Fields_BulkActions", m => BuildOptionsViewModel(m, model))
                .Location("Actions:10.1")
        );
    }

    public override Task<IDisplayResult> UpdateAsync(ListNotificationOptions model, UpdateEditorContext context)
    {
        // Map the incoming values from a form post to the filter result.
        model.FilterResult.MapFrom(model);

        return EditAsync(model, context);
    }

    private static void BuildOptionsViewModel(ListNotificationOptions m, ListNotificationOptions model)
    {
        m.Status = model.Status;
        m.SearchText = model.SearchText;
        m.OriginalSearchText = model.OriginalSearchText;
        m.FilterResult = model.FilterResult;
        m.Sorts = model.Sorts;
        m.Statuses = model.Statuses;
        m.BulkActions = model.BulkActions;
        m.BulkAction = model.BulkAction;
        m.StartIndex = model.StartIndex;
        m.EndIndex = model.EndIndex;
        m.NotificationsCount = model.NotificationsCount;
        m.TotalItemCount = model.TotalItemCount;
        m.OrderBy = model.OrderBy;
        m.FilterResult = model.FilterResult;
    }
}

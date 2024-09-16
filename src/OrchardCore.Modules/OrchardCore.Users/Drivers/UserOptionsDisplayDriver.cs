using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Drivers;

public sealed class UserOptionsDisplayDriver : DisplayDriver<UserIndexOptions>
{
    // Maintain the Options prefix for compatibility with binding.
    protected override void BuildPrefix(UserIndexOptions options, string htmlFieldPrefix)
    {
        Prefix = "Options";
    }

    public override Task<IDisplayResult> DisplayAsync(UserIndexOptions model, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<UserIndexOptions>("UsersAdminListBulkActions", m => BuildUserOptionsViewModel(m, model)).Location("BulkActions", "Content:10"),
            View("UsersAdminFilters_Thumbnail__Name", model).Location("Thumbnail", "Content:10"),
            View("UsersAdminFilters_Thumbnail__Email", model).Location("Thumbnail", "Content:20"),
            View("UsersAdminFilters_Thumbnail__Status", model).Location("Thumbnail", "Content:30"),
            View("UsersAdminFilters_Thumbnail__Role", model).Location("Thumbnail", "Content:40"),
            View("UsersAdminFilters_Thumbnail__Sort", model).Location("Thumbnail", "Content:50")
        );
    }

    public override Task<IDisplayResult> EditAsync(UserIndexOptions model, BuildEditorContext context)
    {
        // Map the filter result to a model so the ui can reflect current selections.
        model.FilterResult.MapTo(model);

        return CombineAsync(
            Initialize<UserIndexOptions>("UsersAdminListSearch", m => BuildUserOptionsViewModel(m, model)).Location("Search:10"),
            Initialize<UserIndexOptions>("UsersAdminListCreate", m => BuildUserOptionsViewModel(m, model)).Location("Create:10"),
            Initialize<UserIndexOptions>("UsersAdminListSummary", m => BuildUserOptionsViewModel(m, model)).Location("Summary:10"),
            Initialize<UserIndexOptions>("UsersAdminListFilters", m => BuildUserOptionsViewModel(m, model)).Location("Actions:10.1"),
            Initialize<UserIndexOptions>("UsersAdminList_Fields_BulkActions", m => BuildUserOptionsViewModel(m, model)).Location("Actions:10.1")
        );
    }

    public override Task<IDisplayResult> UpdateAsync(UserIndexOptions model, UpdateEditorContext context)
    {
        // Map the incoming values from a form post to the filter result.
        model.FilterResult.MapFrom(model);

        return EditAsync(model, context);
    }

    private static void BuildUserOptionsViewModel(UserIndexOptions m, UserIndexOptions model)
    {
        m.SearchText = model.SearchText;
        m.OriginalSearchText = model.OriginalSearchText;
        m.Order = model.Order;
        m.Filter = model.Filter;
        m.BulkAction = model.BulkAction;
        m.UserSorts = model.UserSorts;
        m.UserFilters = model.UserFilters;
        m.UsersBulkAction = model.UsersBulkAction;
        m.UserRoleFilters = model.UserRoleFilters;
        m.StartIndex = model.StartIndex;
        m.EndIndex = model.EndIndex;
        m.UsersCount = model.UsersCount;
        m.TotalItemCount = model.TotalItemCount;
        m.FilterResult = model.FilterResult;
    }
}

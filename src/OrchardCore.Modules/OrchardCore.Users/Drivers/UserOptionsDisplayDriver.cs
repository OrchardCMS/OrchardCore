using System.Threading.Tasks;
using OrchardCore.Users.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Users.Drivers
{
    public class UserOptionsDisplayDriver : DisplayDriver<UserIndexOptions>
    {
        // Maintain the Options prefix for compatability with binding.
        protected override void BuildPrefix(UserIndexOptions options, string htmlFieldPrefix)
        {
            Prefix = "Options";
        }

        public override IDisplayResult Display(UserIndexOptions model)
        {
            return Initialize<UserIndexOptions>("UsersAdminListBulkActions", m => BuildUserOptionsViewModel(m, model)).Location("BulkActions", "Content:10");
        }

        public override IDisplayResult Edit(UserIndexOptions model)
        {
            return Combine(
                Initialize<UserIndexOptions>("UsersAdminListSearch", m => BuildUserOptionsViewModel(m, model)).Location("Search:10"),
                Initialize<UserIndexOptions>("UsersAdminListCreate", m => BuildUserOptionsViewModel(m, model)).Location("Create:10"),
                Initialize<UserIndexOptions>("UsersAdminListSummary", m => BuildUserOptionsViewModel(m, model)).Location("Summary:10"),
                Initialize<UserIndexOptions>("UsersAdminListFilters", m => BuildUserOptionsViewModel(m, model)).Location("Actions:10.1"),
                Initialize<UserIndexOptions>("UsersAdminList_Fields_BulkActions", m => BuildUserOptionsViewModel(m, model)).Location("Actions:10.1")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(UserIndexOptions model, IUpdateModel updater)
        {
            var viewModel = new UserIndexOptions();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                model.RouteValues.TryAdd("Options.Filter", viewModel.Filter);
                model.RouteValues.TryAdd("Options.Order", viewModel.Order);
                model.RouteValues.TryAdd("Options.Search", viewModel.Search);
            }

            return Edit(model);
        }

        private static void BuildUserOptionsViewModel(UserIndexOptions m, UserIndexOptions model)
        {
            m.Search = model.Search;
            m.Order = model.Order;
            m.Filter = model.Filter;
            m.BulkAction = model.BulkAction;

            m.UserSorts = model.UserSorts;
            m.UserFilters = model.UserFilters;

            m.UsersBulkAction = model.UsersBulkAction;
            // m.UserFil = model.CreatableTypes;
            m.StartIndex = model.StartIndex;
            m.EndIndex = model.EndIndex;
            m.UsersCount = model.UsersCount;
            m.TotalItemCount = model.TotalItemCount;
        }
    }
}

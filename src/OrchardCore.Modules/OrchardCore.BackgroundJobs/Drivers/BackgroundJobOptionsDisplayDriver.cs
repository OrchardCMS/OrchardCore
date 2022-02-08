using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundJobs.Drivers
{
    public class BackgroundJobOptionsDisplayDriver : DisplayDriver<BackgroundJobIndexOptions>
    {
        // Maintain the Options prefix for compatability with binding.
        protected override void BuildPrefix(BackgroundJobIndexOptions options, string htmlFieldPrefix)
        {
            Prefix = "Options";
        }

        public override IDisplayResult Display(BackgroundJobIndexOptions model)
        {
            return Combine(
                Initialize<BackgroundJobIndexOptions>("BackgroundJobsAdminListBulkActions", m => BuildBackgroundJobOptionsViewModel(m, model)).Location("BulkActions", "Content:10"),
                View("BackgroundJobsAdminFilters_Thumbnail__Name", model).Location("Thumbnail", "Content:10"),
                View("BackgroundJobsAdminFilters_Thumbnail__Status", model).Location("Thumbnail", "Content:30"),
                View("BackgroundJobsAdminFilters_Thumbnail__Sort", model).Location("Thumbnail", "Content:50")
            );
        }

        public override IDisplayResult Edit(BackgroundJobIndexOptions model)
        {
            // Map the filter result to a model so the ui can reflect current selections.
            model.FilterResult.MapTo(model);
            return Combine(
                Initialize<BackgroundJobIndexOptions>("BackgroundJobsAdminListSearch", m => BuildBackgroundJobOptionsViewModel(m, model)).Location("Search:10"),
                Initialize<BackgroundJobIndexOptions>("BackgroundJobsAdminListSummary", m => BuildBackgroundJobOptionsViewModel(m, model)).Location("Summary:10"),
                Initialize<BackgroundJobIndexOptions>("BackgroundJobsAdminListFilters", m => BuildBackgroundJobOptionsViewModel(m, model)).Location("Actions:10.1"),
                Initialize<BackgroundJobIndexOptions>("BackgroundJobsAdminList_Fields_BulkActions", m => BuildBackgroundJobOptionsViewModel(m, model)).Location("Actions:10.1")
            );
        }

        public override Task<IDisplayResult> UpdateAsync(BackgroundJobIndexOptions model, IUpdateModel updater)
        {
            // Map the incoming values from a form post to the filter result.
            model.FilterResult.MapFrom(model);

            return Task.FromResult<IDisplayResult>(Edit(model));
        }

        private static void BuildBackgroundJobOptionsViewModel(BackgroundJobIndexOptions m, BackgroundJobIndexOptions model)
        {
            m.SearchText = model.SearchText;
            m.OriginalSearchText = model.OriginalSearchText;
            m.Order = model.Order;
            m.Filter = model.Filter;
            m.BulkAction = model.BulkAction;
            m.UserSorts = model.UserSorts;
            m.UserFilters = model.UserFilters;
            m.UsersBulkAction = model.UsersBulkAction;
            m.StartIndex = model.StartIndex;
            m.EndIndex = model.EndIndex;
            m.BackgrounJobsCount = model.BackgrounJobsCount;
            m.TotalItemCount = model.TotalItemCount;
            m.FilterResult = model.FilterResult;
        }
    }
}

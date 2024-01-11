using System.Threading.Tasks;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AuditTrail.Drivers
{
    public class AuditTrailOptionsDisplayDriver : DisplayDriver<AuditTrailIndexOptions>
    {
        // Maintain the Options prefix for compatability with binding.
        protected override void BuildPrefix(AuditTrailIndexOptions options, string htmlFieldPrefix)
        {
            Prefix = "Options";
        }

        public override IDisplayResult Display(AuditTrailIndexOptions model)
        {
            return Combine(
                View("AuditTrailAdminFilters_Thumbnail__Category", model).Location("Thumbnail", "Content:10"),
                View("AuditTrailAdminFilters_Thumbnail__Event", model).Location("Thumbnail", "Content:10"),
                View("AuditTrailAdminFilters_Thumbnail__Date", model).Location("Thumbnail", "Content:20"),
                View("AuditTrailAdminFilters_Thumbnail__UserName", model).Location("Thumbnail", "Content:30"),
                View("AuditTrailAdminFilters_Thumbnail__UserId", model).Location("Thumbnail", "Content:40"),
                View("AuditTrailAdminFilters_Thumbnail__CorrelationId", model).Location("Thumbnail", "Content:50")
            );
        }

        public override IDisplayResult Edit(AuditTrailIndexOptions model)
        {
            // Map the filter result to a model so the ui can reflect current selections.
            model.FilterResult.MapTo(model);
            return Combine(
                Initialize<AuditTrailIndexOptions>("AuditTrailAdminListSearch", m => BuildUserOptionsViewModel(m, model)).Location("Search:10"),
                Initialize<AuditTrailIndexOptions>("AuditTrailAdminListSummary", m => BuildUserOptionsViewModel(m, model)).Location("Summary:10"),
                Initialize<AuditTrailIndexOptions>("AuditTrailAdminListFilters", m => BuildUserOptionsViewModel(m, model)).Location("Actions:10.1")
            );
        }

        public override Task<IDisplayResult> UpdateAsync(AuditTrailIndexOptions model, IUpdateModel updater)
        {
            // Map the incoming values from a form post to the filter result.
            model.FilterResult.MapFrom(model);

            return Task.FromResult<IDisplayResult>(Edit(model));
        }

        private static void BuildUserOptionsViewModel(AuditTrailIndexOptions m, AuditTrailIndexOptions model)
        {
            m.SearchText = model.SearchText;
            m.OriginalSearchText = model.OriginalSearchText;
            m.Category = model.Category;
            m.CorrelationId = model.CorrelationId;
            m.CorrelationIdFromRoute = model.CorrelationIdFromRoute;
            m.Categories = model.Categories;
            m.Event = model.Event;
            m.Events = model.Events;
            m.Sort = model.Sort;
            m.AuditTrailSorts = model.AuditTrailSorts;
            m.Date = model.Date;
            m.AuditTrailDates = model.AuditTrailDates;
            m.StartIndex = model.StartIndex;
            m.EndIndex = model.EndIndex;
            m.EventsCount = model.EventsCount;
            m.TotalItemCount = model.TotalItemCount;
            m.FilterResult = model.FilterResult;
        }
    }
}

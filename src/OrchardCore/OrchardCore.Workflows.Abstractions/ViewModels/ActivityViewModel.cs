using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.ViewModels
{
    public class ActivityViewModel<TActivity> : ShapeViewModel where TActivity : IActivity
    {
        public ActivityViewModel()
        {
        }

        public ActivityViewModel(TActivity activity)
        {
            Activity = activity;
        }

        [BindNever]
        public TActivity Activity { get; set; }
    }
}

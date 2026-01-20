using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public abstract class TenantTaskDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel>
        where TActivity : TenantTask where TViewModel : TenantTaskViewModel<TActivity>, new()
    {
        public string TenantName { get; set; }

        public override IDisplayResult Display(TActivity activity)
        {
            return Combine(
                Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new TenantTaskViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
                Shape($"{typeof(TActivity).Name}_Fields_Design", new TenantTaskViewModel<TActivity>(activity)).Location("Design", "Content")
            );
        }
    }
}

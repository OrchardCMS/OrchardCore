using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Tenants.Workflows.Drivers;

public abstract class TenantTaskDisplayDriver<TActivity, TViewModel> : ActivityDisplayDriver<TActivity, TViewModel>
    where TActivity : TenantTask where TViewModel : TenantTaskViewModel<TActivity>, new()
{
    public string TenantName { get; set; }

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory($"{typeof(TActivity).Name}_Fields_Thumbnail", static (TActivity a) => new TenantTaskViewModel<TActivity>(a), activity).Location("Thumbnail", "Content"),
            Factory($"{typeof(TActivity).Name}_Fields_Design", static (TActivity a) => new TenantTaskViewModel<TActivity>(a), activity).Location("Design", "Content")
        );
    }
}

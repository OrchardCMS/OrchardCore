using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.Services;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Tenants.Workflows.Drivers
{
    public class TenantSetupEventDisplay : ActivityDisplayDriver<TenantSetupEvent, TenantSetupEventViewModel>
    {
        //public TenantSetupEventDisplay(IUserService userService)
        //{
        //    UserService = userService;
        //}

        //protected IUserService UserService { get; }

        protected override void EditActivity(TenantSetupEvent source, TenantSetupEventViewModel target)
        {
        }

        public override IDisplayResult Display(TenantSetupEvent activity)
        {
            return Combine(
                Shape("TenantSetupEvent_Fields_Thumbnail", new TenantSetupEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory("TenantSetupEvent_Fields_Design", ctx =>
                {
                    var shape = new TenantSetupEventViewModel();
                    shape.Activity = activity;

                    return shape;
                }).Location("Design", "Content")
            );
        }
    }
}

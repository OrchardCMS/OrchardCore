using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserEnabledEventDisplayDriver : ActivityDisplayDriver<UserEnabledEvent, UserEnabledEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserEnabledEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape("UserEnabledEvent_Fields_Thumbnail", new UserEnabledEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserEnabledEvent_Fields_Design", ctx =>
            {
                var shape = new UserEnabledEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

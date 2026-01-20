using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public class UserConfirmedEventDisplayDriver : ActivityDisplayDriver<UserConfirmedEvent, UserConfirmedEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserConfirmedEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape("UserConfirmedEvent_Fields_Thumbnail", new UserConfirmedEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserConfirmedEvent_Fields_Design", ctx =>
            {
                var shape = new UserConfirmedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

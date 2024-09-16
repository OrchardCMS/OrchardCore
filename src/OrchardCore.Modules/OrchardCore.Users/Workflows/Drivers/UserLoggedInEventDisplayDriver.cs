using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserLoggedInEventDisplayDriver : ActivityDisplayDriver<UserLoggedInEvent, UserLoggedInEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserLoggedInEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape("UserLoggedInEvent_Fields_Thumbnail", new UserLoggedInEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserLoggedInEvent_Fields_Design", ctx =>
            {
                var shape = new UserLoggedInEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

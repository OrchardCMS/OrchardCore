using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserLoggedOutEventDisplayDriver : ActivityDisplayDriver<UserLoggedOutEvent, UserLoggedOutEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserLoggedOutEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory("UserLoggedOutEvent_Fields_Thumbnail", static (UserLoggedOutEvent a) => new UserLoggedOutEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserLoggedOutEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserLoggedOutEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

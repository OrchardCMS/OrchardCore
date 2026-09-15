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
            Factory("UserLoggedInEvent_Fields_Thumbnail", static (UserLoggedInEvent a) => new UserLoggedInEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserLoggedInEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserLoggedInEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

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
            Factory("UserConfirmedEvent_Fields_Thumbnail", static (UserConfirmedEvent a) => new UserConfirmedEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserConfirmedEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserConfirmedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

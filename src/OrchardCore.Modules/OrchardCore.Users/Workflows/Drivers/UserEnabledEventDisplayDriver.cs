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
            Factory("UserEnabledEvent_Fields_Thumbnail", static (UserEnabledEvent a) => new UserEnabledEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserEnabledEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserEnabledEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

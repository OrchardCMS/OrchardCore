using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserDisabledEventDisplayDriver : ActivityDisplayDriver<UserDisabledEvent, UserDisabledEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserDisabledEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory("UserDisabledEvent_Fields_Thumbnail", static (UserDisabledEvent a) => new UserDisabledEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserDisabledEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserDisabledEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

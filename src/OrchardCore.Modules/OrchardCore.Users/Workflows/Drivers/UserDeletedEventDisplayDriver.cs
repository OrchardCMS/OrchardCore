using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserDeletedEventDisplayDriver : ActivityDisplayDriver<UserDeletedEvent, UserDeletedEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserDeletedEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory("UserDeletedEvent_Fields_Thumbnail", static (UserDeletedEvent a) => new UserDeletedEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserDeletedEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserDeletedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

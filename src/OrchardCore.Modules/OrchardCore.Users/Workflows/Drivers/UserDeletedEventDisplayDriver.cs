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
            Shape("UserDeletedEvent_Fields_Thumbnail", new UserDeletedEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserDeletedEvent_Fields_Design", ctx =>
            {
                var shape = new UserDeletedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

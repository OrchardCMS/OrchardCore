using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserUpdatedEventDisplayDriver : ActivityDisplayDriver<UserUpdatedEvent, UserUpdatedEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserUpdatedEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape("UserUpdatedEvent_Fields_Thumbnail", new UserUpdatedEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserUpdatedEvent_Fields_Design", ctx =>
            {
                var shape = new UserUpdatedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

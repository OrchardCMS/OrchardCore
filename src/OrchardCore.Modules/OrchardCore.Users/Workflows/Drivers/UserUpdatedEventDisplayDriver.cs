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
            Factory("UserUpdatedEvent_Fields_Thumbnail", static (UserUpdatedEvent a) => new UserUpdatedEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserUpdatedEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserUpdatedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public sealed class UserCreatedEventDisplayDriver : ActivityDisplayDriver<UserCreatedEvent, UserCreatedEventViewModel>
{
    public override Task<IDisplayResult> DisplayAsync(UserCreatedEvent activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory("UserCreatedEvent_Fields_Thumbnail", static (UserCreatedEvent a) => new UserCreatedEventViewModel(a), activity).Location("Thumbnail", "Content"),
            Factory("UserCreatedEvent_Fields_Design", static (ctx, activity) =>
            {
                var shape = new UserCreatedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }, activity).Location("Design", "Content")
        );
    }
}

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
            Shape("UserDisabledEvent_Fields_Thumbnail", new UserDisabledEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserDisabledEvent_Fields_Design", ctx =>
            {
                var shape = new UserDisabledEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

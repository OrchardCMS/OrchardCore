using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers;

public class UserConfirmedEventDisplayDriver : ActivityDisplayDriver<UserConfirmedEvent, UserConfirmedEventViewModel>
{
    public UserConfirmedEventDisplayDriver(IUserService userService)
    {
        UserService = userService;
    }

    protected IUserService UserService { get; }

    protected override void EditActivity(UserConfirmedEvent source, UserConfirmedEventViewModel target)
    {
    }

    public override IDisplayResult Display(UserConfirmedEvent activity)
    {
        return Combine(
            Shape("UserConfirmedEvent_Fields_Thumbnail", new UserConfirmedEventViewModel(activity)).Location("Thumbnail", "Content"),
            Factory("UserConfirmedEvent_Fields_Design", ctx =>
            {
                var shape = new UserConfirmedEventViewModel
                {
                    Activity = activity,
                };

                return shape;
            }).Location("Design", "Content")
        );
    }
}

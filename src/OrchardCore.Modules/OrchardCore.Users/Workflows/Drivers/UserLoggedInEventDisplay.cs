using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserLoggedInEventDisplay : ActivityDisplayDriver<UserLoggedInEvent, UserLoggedInEventViewModel>
    {
        public UserLoggedInEventDisplay(IUserService userService)
        {
            UserService = userService;
        }

        protected IUserService UserService { get; }

        protected override void EditActivity(UserLoggedInEvent source, UserLoggedInEventViewModel target)
        {
        }

        public override IDisplayResult Display(UserLoggedInEvent activity)
        {
            return Combine(
                Shape("UserLoggedInEvent_Fields_Thumbnail", new UserLoggedInEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory("UserLoggedInEvent_Fields_Design", ctx =>
                {
                    var shape = new UserLoggedInEventViewModel();
                    shape.Activity = activity;

                    return shape;
                }).Location("Design", "Content")
            );
        }
    }
}

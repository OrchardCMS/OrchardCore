using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserEnabledEventDisplay : ActivityDisplayDriver<UserEnabledEvent, UserEnabledEventViewModel>
    {
        public UserEnabledEventDisplay(IUserService userService)
        {
            UserService = userService;
        }

        protected IUserService UserService { get; }

        protected override void EditActivity(UserEnabledEvent source, UserEnabledEventViewModel target)
        {
        }

        public override IDisplayResult Display(UserEnabledEvent activity)
        {
            return Combine(
                Shape("UserEnabledEvent_Fields_Thumbnail", new UserEnabledEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory("UserEnabledEvent_Fields_Design", ctx =>
                {
                    var shape = new UserEnabledEventViewModel();
                    shape.Activity = activity;

                    return shape;
                }).Location("Design", "Content")
            );
        }
    }
}

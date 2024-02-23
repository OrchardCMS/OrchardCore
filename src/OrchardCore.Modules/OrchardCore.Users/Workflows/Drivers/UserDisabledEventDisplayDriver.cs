using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserDisabledEventDisplayDriver : ActivityDisplayDriver<UserDisabledEvent, UserDisabledEventViewModel>
    {
        public UserDisabledEventDisplayDriver(IUserService userService)
        {
            UserService = userService;
        }

        protected IUserService UserService { get; }

        protected override void EditActivity(UserDisabledEvent source, UserDisabledEventViewModel target)
        {
        }

        public override IDisplayResult Display(UserDisabledEvent activity)
        {
            return Combine(
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
}

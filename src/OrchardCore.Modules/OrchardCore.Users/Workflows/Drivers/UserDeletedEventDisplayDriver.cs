using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserDeletedEventDisplayDriver : ActivityDisplayDriver<UserDeletedEvent, UserDeletedEventViewModel>
    {
        public UserDeletedEventDisplayDriver(IUserService userService)
        {
            UserService = userService;
        }

        protected IUserService UserService { get; }

        protected override void EditActivity(UserDeletedEvent source, UserDeletedEventViewModel target)
        {
        }

        public override IDisplayResult Display(UserDeletedEvent activity)
        {
            return Combine(
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
}

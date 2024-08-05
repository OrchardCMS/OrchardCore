using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserLoggedInEventDisplayDriver : ActivityDisplayDriver<UserLoggedInEvent, UserLoggedInEventViewModel>
    {
        protected IUserService UserService { get; }

        public UserLoggedInEventDisplayDriver(IUserService userService)
        {
            UserService = userService;
        }

        public override Task<IDisplayResult> DisplayAsync(UserLoggedInEvent activity, BuildDisplayContext context)
        {
            return CombineAsync(
                Shape("UserLoggedInEvent_Fields_Thumbnail", new UserLoggedInEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory("UserLoggedInEvent_Fields_Design", ctx =>
                {
                    var shape = new UserLoggedInEventViewModel
                    {
                        Activity = activity,
                    };

                    return shape;
                }).Location("Design", "Content")
            );
        }
    }
}

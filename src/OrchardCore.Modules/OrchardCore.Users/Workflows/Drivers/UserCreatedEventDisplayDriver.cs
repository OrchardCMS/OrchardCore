using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserCreatedEventDisplayDriver : ActivityDisplayDriver<UserCreatedEvent, UserCreatedEventViewModel>
    {
        protected IUserService UserService { get; }

        public UserCreatedEventDisplayDriver(IUserService userService)
        {
            UserService = userService;
        }

        public override Task<IDisplayResult> DisplayAsync(UserCreatedEvent activity, BuildDisplayContext context)
        {
            return CombineAsync(
                Shape("UserCreatedEvent_Fields_Thumbnail", new UserCreatedEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory("UserCreatedEvent_Fields_Design", ctx =>
                {
                    var shape = new UserCreatedEventViewModel
                    {
                        Activity = activity,
                    };

                    return shape;
                }).Location("Design", "Content")
            );
        }
    }
}

using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Users.Workflows.ViewModels;
using OrchardCore.Workflows.Display;

namespace OrchardCore.Users.Workflows.Drivers
{
    public class UserCreatedEventDisplay : ActivityDisplayDriver<UserCreatedEvent, UserCreatedEventViewModel>
    {
        public UserCreatedEventDisplay(IUserService userService)
        {
            UserService = userService;
        }

        protected IUserService UserService { get; }

        protected override void EditActivity(UserCreatedEvent source, UserCreatedEventViewModel target)
        {
        }

        public override IDisplayResult Display(UserCreatedEvent activity)
        {
            return Combine(
                Shape($"{nameof(UserCreatedEvent)}_Fields_Thumbnail", new UserCreatedEventViewModel(activity)).Location("Thumbnail", "Content"),
                Factory($"{nameof(UserCreatedEvent)}_Fields_Design", ctx =>
                {
                    var shape = new UserCreatedEventViewModel();
                    shape.Activity = activity;

                    return shape;

                }).Location("Design", "Content")
            );
        }
    }
}

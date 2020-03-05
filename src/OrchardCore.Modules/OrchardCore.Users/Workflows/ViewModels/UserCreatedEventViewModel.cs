using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserCreatedEventViewModel : UserEventViewModel<UserCreatedEvent>
    {
        public UserCreatedEventViewModel()
        {
        }

        public UserCreatedEventViewModel(UserCreatedEvent activity)
        {
            Activity = activity;
        }
    }
}

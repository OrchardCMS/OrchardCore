using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserLoggedInEventViewModel : UserEventViewModel<UserLoggedInEvent>
    {
        public UserLoggedInEventViewModel()
        {
        }

        public UserLoggedInEventViewModel(UserLoggedInEvent activity)
        {
            Activity = activity;
        }
    }
}

using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserEnabledEventViewModel : UserEventViewModel<UserEnabledEvent>
    {
        public UserEnabledEventViewModel()
        {
        }

        public UserEnabledEventViewModel(UserEnabledEvent activity)
        {
            Activity = activity;
        }
    }
}

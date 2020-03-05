using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserDisabledEventViewModel : UserEventViewModel<UserDisabledEvent>
    {
        public UserDisabledEventViewModel()
        {
        }

        public UserDisabledEventViewModel(UserDisabledEvent activity)
        {
            Activity = activity;
        }
    }
}

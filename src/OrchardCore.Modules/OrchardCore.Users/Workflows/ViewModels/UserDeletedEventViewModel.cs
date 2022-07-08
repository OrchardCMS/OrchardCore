using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserDeletedEventViewModel : UserEventViewModel<UserDeletedEvent>
    {
        public UserDeletedEventViewModel()
        {
        }

        public UserDeletedEventViewModel(UserDeletedEvent activity)
        {
            Activity = activity;
        }
    }
}

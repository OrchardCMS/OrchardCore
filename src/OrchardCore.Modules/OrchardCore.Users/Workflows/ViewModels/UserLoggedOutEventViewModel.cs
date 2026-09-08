using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels;

public class UserLoggedOutEventViewModel : UserEventViewModel<UserLoggedOutEvent>
{
    public UserLoggedOutEventViewModel()
    {
    }

    public UserLoggedOutEventViewModel(UserLoggedOutEvent activity)
    {
        Activity = activity;
    }
}

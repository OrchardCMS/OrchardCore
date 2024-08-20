using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels;

public class UserConfirmedEventViewModel : UserEventViewModel<UserConfirmedEvent>
{
    public UserConfirmedEventViewModel()
    {
    }

    public UserConfirmedEventViewModel(UserConfirmedEvent activity)
        : base(activity)
    {
    }
}

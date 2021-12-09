using OrchardCore.Users.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class UserEventViewModel<T> : ActivityViewModel<T> where T : UserEvent
    {
        public UserEventViewModel()
        {
        }

        public UserEventViewModel(T activity)
        {
            Activity = activity;
        }
    }
}

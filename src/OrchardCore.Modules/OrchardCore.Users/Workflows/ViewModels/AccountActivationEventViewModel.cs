using System.ComponentModel.DataAnnotations;
using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class AccountActivationEventViewModel : UserEventViewModel<AccountActivationEvent>
    {
        public AccountActivationEventViewModel()
        {

        }

        public AccountActivationEventViewModel(AccountActivationEvent activity) : base(activity)
        {

        }

        [Required]
        public string PropertyName { get; set; }
    }
}

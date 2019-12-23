using System.ComponentModel.DataAnnotations;
using OrchardCore.Users.Workflows.Activities;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class AccountActivatedEventViewModel : UserEventViewModel<AccountActivatedEvent>
    {
        public AccountActivatedEventViewModel()
        {

        }

        public AccountActivatedEventViewModel(AccountActivatedEvent activity) : base(activity)
        {

        }

        [Required]
        public string PropertyName { get; set; }
    }
}

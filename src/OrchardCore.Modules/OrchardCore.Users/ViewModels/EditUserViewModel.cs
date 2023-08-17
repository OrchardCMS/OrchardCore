using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserViewModel
    {
        public bool EmailConfirmed { get; set; }

        public bool IsEnabled { get; set; }

        public string Password { get; set; }

        public string PasswordConfirmation { get; set; }

        /// <summary>
        /// When a user only has rights to view they cannot update this model.
        /// </summary>
        [BindNever]
        public bool IsEditingDisabled { get; set; }

        [BindNever]
        public bool IsNewRequest { get; set; }

    }
}

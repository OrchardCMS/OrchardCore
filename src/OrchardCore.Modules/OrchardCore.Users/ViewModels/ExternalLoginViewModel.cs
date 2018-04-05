using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class ExternalLoginViewModel : RegisterViewModel
    {
        bool ExistingUser { get; set; }
    }
}

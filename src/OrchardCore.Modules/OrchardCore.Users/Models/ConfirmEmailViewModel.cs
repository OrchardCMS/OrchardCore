using OrchardCore.Users.Models;

namespace OrchardCore.Users.ViewModels
{
    public class ConfirmEmailViewModel
    {        
        public IUser User { get; set; }
        public string ConfirmEmailUrl { get; set; }
    }
} 
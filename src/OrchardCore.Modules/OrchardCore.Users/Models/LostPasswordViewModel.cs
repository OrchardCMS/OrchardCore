using OrchardCore.Users.Models;

namespace OrchardCore.Users.ViewModels
{
    public class LostPasswordViewModel
    {        
        public User User { get; set; }
        public string LostPasswordUrl { get; set; }
    }
} 
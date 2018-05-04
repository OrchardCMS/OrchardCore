using OrchardCore.Users.Models;

namespace OrchardCore.UserProfile.Models
{
    public class UserProfile : User
    {
        public string TimeZone { get; set; }
    }
}

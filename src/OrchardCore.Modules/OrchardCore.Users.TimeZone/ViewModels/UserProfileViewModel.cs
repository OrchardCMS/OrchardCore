using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.ViewModels
{
    public class UserProfileViewModel
    {
        public string TimeZone { get; set; }
        public ITimeZone[] TimeZones { get; set; }
    }
}

using OrchardCore.Modules;

namespace OrchardCore.UserProfile.ViewModels
{
    public class EditUserProfileViewModel
    {
        public string TimeZone { get; set; }
        public ITimeZone[] TimeZones { get; set; }
    }
}

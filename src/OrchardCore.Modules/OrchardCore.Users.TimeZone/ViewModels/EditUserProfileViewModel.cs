using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.ViewModels
{
    public class EditUserProfileViewModel
    {
        public string TimeZone { get; set; }
        public ITimeZone[] TimeZones { get; set; }
    }
}

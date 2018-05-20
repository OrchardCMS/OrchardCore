using System.Collections.Generic;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.ViewModels
{
    public class UserProfileViewModel
    {
        public string TimeZone { get; set; }
        public IList<ITimeZone> TimeZones { get; set; }
    }
}

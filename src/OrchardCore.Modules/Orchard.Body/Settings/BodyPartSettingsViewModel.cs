using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Body.Settings
{
    public class BodyPartSettingsViewModel
    {
        public string Editor { get; set; }

        [BindNever]
        public BodyPartSettings BodyPartSettings { get; set; }
    }
}

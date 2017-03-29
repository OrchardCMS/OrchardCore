using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.Body.Settings
{
    public class BodyPartSettingsViewModel
    {
        public bool RenderTokens { get; set; }

        public string Editor { get; set; }

        [BindNever]
        public BodyPartSettings BodyPartSettings { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Templates.Cms.Module.Settings
{
    public class MyTestPartSettingsViewModel
    {
        public string MySetting { get; set; }

        [BindNever]
        public MyTestPartSettings MyTestPartSettings { get; set; }
    }
}

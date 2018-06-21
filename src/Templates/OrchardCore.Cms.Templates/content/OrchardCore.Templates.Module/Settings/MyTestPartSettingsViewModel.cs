using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Templates.Module.Settings
{
    public class MyTestPartSettingsViewModel
    {
        public string MySetting { get; set; }

        [BindNever]
        public MyTestPartSettings MyTestPartSettings { get; set; }
    }
}

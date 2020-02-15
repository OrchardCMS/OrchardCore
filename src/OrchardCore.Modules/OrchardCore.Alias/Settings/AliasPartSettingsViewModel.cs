using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Alias.Settings
{
    public class AliasPartSettingsViewModel
    {
        public string Pattern { get; set; }
        public AliasPartOptions Options { get; set; }
        [BindNever]
        public AliasPartSettings AliasPartSettings { get; set; }
    }
}

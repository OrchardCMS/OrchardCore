using Shortcodes;

namespace OrchardCore.Shortcodes.ViewModels
{
    public class ShortcodeViewModel
    {
        public Arguments Args { get; set; }
        public string Content { get; set; }
        public Context Context { get; set; }
    }
}

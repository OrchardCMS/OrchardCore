using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Title.Models;

namespace OrchardCore.Title.ViewModels
{
    public class TitlePartSettingsViewModel
    {
        public TitlePartOptions Options { get; set; }
        public string Pattern { get; set; }

        [BindNever]
        public TitlePartSettings TitlePartSettings { get; set; }
    }
}

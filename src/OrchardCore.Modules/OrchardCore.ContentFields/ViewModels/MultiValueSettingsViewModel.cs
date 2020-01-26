using OrchardCore.ContentFields.Settings;

namespace OrchardCore.ContentFields.ViewModels
{
    public class MultiValueSettingsViewModel
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public MultiValueEditorOption Editor { get; set; }
        public string Options { get; set; }
        public string DefaultValue { get; set; }
    }
}

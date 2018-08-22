using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentFields.Settings
{
    public class TextFieldPredefinedListEditorSettings
    {
        public string Options { get; set; }
        public string Editor { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
    }
}
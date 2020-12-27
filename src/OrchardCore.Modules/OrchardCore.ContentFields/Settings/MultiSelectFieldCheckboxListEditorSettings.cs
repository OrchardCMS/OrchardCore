using Newtonsoft.Json;

namespace OrchardCore.ContentFields.Settings
{
    public class MultiSelectFieldCheckboxListEditorSettings
    {
        public CheckboxDirection Direction { get; set; }
    }

    public enum CheckboxDirection
    {
        Vertical,
        Horizontal
    }
}

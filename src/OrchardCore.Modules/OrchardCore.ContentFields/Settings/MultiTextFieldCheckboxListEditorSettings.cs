namespace OrchardCore.ContentFields.Settings
{
    public class MultiTextFieldCheckboxListEditorSettings
    {
        public CheckboxDirection Direction { get; set; }
    }

    public enum CheckboxDirection
    {
        Vertical,
        Horizontal
    }
}

namespace OrchardCore.ContentFields.ViewModels;

public class VueMultiselectItemViewModel
{
    public string Id { get; set; }
    public string DisplayText { get; set; }
    public bool HasPublished { get; set; }
    public bool IsViewable { get; set; }
    public bool IsEditable { get; set; }
    public bool IsClickable => IsEditable || IsViewable;
}

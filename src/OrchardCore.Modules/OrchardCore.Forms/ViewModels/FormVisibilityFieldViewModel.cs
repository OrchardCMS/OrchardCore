namespace OrchardCore.Forms.ViewModels;

public class FormVisibilityFieldViewModel
{
    public string Name { get; set; }

    public string Value { get; set; }

    public bool IsPredefinedValues { get; set; }

    public string[] PredefinedValues { get; set; }

    public string[] SelectedValues { get; set; }
}

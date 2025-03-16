namespace OrchardCore.Forms.ViewModels;

public class TextAreaPartEditViewModel
{
    private const int DefaultRows = 10;

    private int _rows;

    public string DefaultValue { get; set; }

    public string Placeholder { get; set; }

    public int Rows
    {
        get => _rows > 0
            ? _rows
            : DefaultRows;
        set => _rows = value;
    }
}

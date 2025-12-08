#nullable enable

namespace OrchardCore.Media.Models;

internal sealed class MediaCommands
{
    public const string WidthCommand = "width";
    public const string HeightCommand = "height";
    public const string ResizeModeCommand = "rmode";
    public const string ResizeFocalPointCommand = "rxy";
    public const string FormatCommand = "format";
    public const string BackgroundColorCommand = "bgcolor";
    public const string QualityCommand = "quality";

    // Backing properties (null means "not set").
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? ResizeMode { get; set; }
    public string? ResizeFocalPoint { get; set; }
    public string? Format { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Quality { get; set; }

    public MediaCommands() { }

    public void SetCommands(IEnumerable<KeyValuePair<string, string>> commands)
    {
        foreach (var (key, value) in commands)
        {
            switch (key)
            {
                case WidthCommand:
                    Width = value;
                    break;
                case HeightCommand:
                    Height = value;
                    break;
                case ResizeModeCommand:
                    ResizeMode = value;
                    break;
                case ResizeFocalPointCommand:
                    ResizeFocalPoint = value;
                    break;
                case FormatCommand:
                    Format = value;
                    break;
                case QualityCommand:
                    Quality = value;
                    break;
                case BackgroundColorCommand:
                    BackgroundColor = value;
                    break;
                default:
                    // Unknown keys are ignored.
                    break;
            }
        }
    }

    public IEnumerable<KeyValuePair<string, string>> GetValues()
    {
        if (!string.IsNullOrWhiteSpace(Width))
        {
            yield return new KeyValuePair<string, string>(WidthCommand, Width);
        }
        if (!string.IsNullOrWhiteSpace(Height))
        {
            yield return new KeyValuePair<string, string>(HeightCommand, Height);
        }
        if (!string.IsNullOrWhiteSpace(ResizeMode))
        {
            yield return new KeyValuePair<string, string>(ResizeModeCommand, ResizeMode);
        }
        if (!string.IsNullOrWhiteSpace(ResizeFocalPoint))
        {
            yield return new KeyValuePair<string, string>(ResizeFocalPointCommand, ResizeFocalPoint);
        }
        if (!string.IsNullOrWhiteSpace(Format))
        {
            yield return new KeyValuePair<string, string>(FormatCommand, Format);
        }
        if (!string.IsNullOrWhiteSpace(BackgroundColor))
        {
            yield return new KeyValuePair<string, string>(BackgroundColorCommand, BackgroundColor);
        }
        if (!string.IsNullOrWhiteSpace(Quality))
        {
            yield return new KeyValuePair<string, string>(QualityCommand, Quality);
        }
    }
}


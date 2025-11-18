#nullable enable

namespace OrchardCore.Media.Models;

public sealed class MediaCommands
{
    private const int Width = 0;
    public const string WidthCommand = "width";

    private const int Height = 1;
    public const string HeightCommand = "height";

    private const int ResizeMode = 2;
    public const string ResizeModeCommand = "rmode";

    private const int ResizeFocalPoint = 3;
    public const string ResizeFocalPointCommand = "rxy";

    private const int Format = 4;
    public const string FormatCommand = "format";

    private const int BackgroundColor = 5;
    public const string BackgroundColorCommand = "bgcolor";

    private const int Quality = 6;
    public const string QualityCommand = "quality";

    private const int CommandCount = 7;

    // Array index corresponds to the integer constants above.
    // Null means "not set".
    private readonly string?[] _values = new string?[CommandCount];

    // Index -> command name (kept in ascending order for predictable enumeration).
    private static readonly string[] _indexToName =
    {
        WidthCommand,
        HeightCommand,
        ResizeModeCommand,
        ResizeFocalPointCommand,
        FormatCommand,
        BackgroundColorCommand,
        QualityCommand,
    };

    public MediaCommands() { }

    public void SetCommands(IEnumerable<KeyValuePair<string, string>> commands)
    {
        foreach (var (key, value) in commands)
        {
            switch (key)
            {
                case WidthCommand:
                    _values[Width] = value;
                    break;
                case HeightCommand:
                    _values[Height] = value;
                    break;
                case ResizeModeCommand:
                    _values[ResizeMode] = value;
                    break;
                case ResizeFocalPointCommand:
                    _values[ResizeFocalPoint] = value;
                    break;
                case FormatCommand:
                    _values[Format] = value;
                    break;
                case QualityCommand:
                    _values[Quality] = value;
                    break;
                case BackgroundColorCommand:
                    _values[BackgroundColor] = value;
                    break;
                default:
                    // Unknown keys are ignored.
                    break;
            }
        }
    }

    public void SetWidth(string value) => _values[Width] = value;

    public void SetHeight(string value) => _values[Height] = value;

    public void SetResizeMode(string value) => _values[ResizeMode] = value;

    public void SetResizeFocalPoint(string value) => _values[ResizeFocalPoint] = value;

    public void SetFormat(string value) => _values[Format] = value;

    public void SetBackgroundColor(string value) => _values[BackgroundColor] = value;

    public void SetQuality(string value) => _values[Quality] = value;

    public IEnumerable<KeyValuePair<string, string>> GetValues()
    {
        for (var i = 0; i < CommandCount; i++)
        {
            var v = _values[i];
            if (v is not null)
            {
                yield return new KeyValuePair<string, string>(_indexToName[i], v);
            }
        }
    }
}


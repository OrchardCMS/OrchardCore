namespace OrchardCore.Media.Models;

public class MediaCommands
{
    private SortedDictionary<int, string> _commands = new();

    public const int Width = 0;
    public const string WidthCommand = "width";

    public const int Height = 1;
    public const string HeightCommand = "height";

    public const int ResizeMode = 2;
    public const string ResizeModeCommand = "rmode";

    public const int ResizeFocalPoint = 3;
    public const string ResizeFocalPointCommand = "rxy";

    public const int Format = 4;
    public const string FormatCommand = "format";

    public const int BackgroundColor = 5;
    public const string BackgroundColorCommand = "bgcolor";

    public const int Quality = 6;
    public const string QualityCommand = "quality";

    public MediaCommands() { }

    public void SetCommands(IEnumerable<KeyValuePair<string, string>> commands)
    {
        foreach (var command in commands)
        {
            switch (command.Key)
            {
                case WidthCommand:
                    SetWidth(command.Value);
                    break;
                case HeightCommand:
                    SetHeight(command.Value);
                    break;
                case ResizeModeCommand:
                    SetResizeMode(command.Value);
                    break;
                case ResizeFocalPointCommand:
                    SetResizeFocalPoint(command.Value);
                    break;
                case FormatCommand:
                    SetFormat(command.Value);
                    break;
                case BackgroundColorCommand:
                    SetBackgroundColor(command.Value);
                    break;
                case QualityCommand:
                    SetQuality(command.Value);
                    break;
                default:
                    break;
            }
        }
    }

    public void SetWidth(string value)
    {
        _commands[Width] = value;
    }

    public void SetHeight(string value)
    {
        _commands[Height] = value;
    }

    public void SetResizeMode(string value)
    {
        _commands[ResizeMode] = value;
    }

    public void SetResizeFocalPoint(string value)
    {
        _commands[ResizeFocalPoint] = value;
    }

    public void SetFormat(string value)
    {
        _commands[Format] = value;
    }

    public void SetBackgroundColor(string value)
    {
        _commands[BackgroundColor] = value;
    }

    public void SetQuality(string value)
    {
        _commands[Quality] = value;
    }

    public IEnumerable<KeyValuePair<string, string>> GetValues()
    {
        foreach (var item in _commands.Keys)
        {
            switch (item)
            {
                case Width:
                    yield return new KeyValuePair<string, string>(WidthCommand, _commands[item]);
                    break;
                case Height:
                    yield return new KeyValuePair<string, string>(HeightCommand, _commands[item]);
                    break;
                case ResizeMode:
                    yield return new KeyValuePair<string, string>(ResizeModeCommand, _commands[item]);
                    break;
                case ResizeFocalPoint:
                    yield return new KeyValuePair<string, string>(ResizeFocalPointCommand, _commands[item]);
                    break;
                case Format:
                    yield return new KeyValuePair<string, string>(FormatCommand, _commands[item]);
                    break;
                case BackgroundColor:
                    yield return new KeyValuePair<string, string>(BackgroundColorCommand, _commands[item]);
                    break;
                case Quality:
                    yield return new KeyValuePair<string, string>(QualityCommand, _commands[item]);
                    break;
                default:
                    break;
            }
        }
    }
}


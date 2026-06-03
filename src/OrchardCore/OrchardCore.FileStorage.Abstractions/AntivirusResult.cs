namespace OrchardCore.FileStorage;

public sealed class AntivirusResult
{
    private AntivirusResult(bool isClean, string message, string threatName)
    {
        IsClean = isClean;
        Message = message;
        ThreatName = threatName;
    }

    public bool IsClean { get; }

    public string Message { get; }

    public string ThreatName { get; }

    public static AntivirusResult Clean { get; } = new(true, null, null);

    public static AntivirusResult Unsafe(string message, string threatName = null) => new(false, message, threatName);
}

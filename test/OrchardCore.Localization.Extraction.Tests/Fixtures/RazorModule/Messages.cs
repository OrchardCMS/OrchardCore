using Microsoft.Extensions.Localization;

namespace Fixture;

public sealed class Messages
{
    private readonly IStringLocalizer _words;

    public Messages(IStringLocalizer<Messages> injected)
    {
        _words = injected;
    }

    public string Welcome => _words["C# message {0}", 42];
}

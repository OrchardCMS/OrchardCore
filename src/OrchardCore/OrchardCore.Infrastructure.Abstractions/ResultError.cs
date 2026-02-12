using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

public class ResultError
{
    public string Key { get; set; } = string.Empty;

    public LocalizedString Message { get; set; }
}

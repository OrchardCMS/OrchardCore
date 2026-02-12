using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

public class ResultError
{
    public string Key { get; set; }

    public LocalizedString Message { get; set; }
}

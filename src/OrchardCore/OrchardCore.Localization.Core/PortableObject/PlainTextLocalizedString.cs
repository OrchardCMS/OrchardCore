using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.PortableObject;

internal sealed class PlainTextLocalizedString : LocalizedString
{
    public PlainTextLocalizedString(string name, string value) : base(name, value)
    {
    }
}

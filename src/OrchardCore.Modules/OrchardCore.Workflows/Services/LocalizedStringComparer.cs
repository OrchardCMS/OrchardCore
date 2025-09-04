using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Services;

public class LocalizedStringComparer : IEqualityComparer<LocalizedString>
{
    public bool Equals(LocalizedString x, LocalizedString y)
    {
        return x.Name.Equals(y.Name, StringComparison.Ordinal);
    }

    public int GetHashCode(LocalizedString obj)
    {
        return obj.Name.GetHashCode();
    }
}

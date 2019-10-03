using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization
{
    // TODO: We can inherit from IStringLocalizer or IPluralizeStringLocalizer
    public interface IDataLocalizer
    {
        LocalizedString this[string name] { get; }

        LocalizedString this[string name, params object[] arguments] { get; }

        IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures);
    }
}
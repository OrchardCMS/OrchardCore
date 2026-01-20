using System.Collections.Generic;

namespace OrchardCore.Localization.Data;

public interface IDataLocalizer
{
    DataLocalizedString this[string name, string context] { get; }

    DataLocalizedString this[string name, string context, params object[] arguments] { get; }

    IEnumerable<DataLocalizedString> GetAllStrings(string context, bool includeParentCultures);
}

using System.Collections.Generic;

namespace OrchardCore.Localization.Data;

public interface ILocalizationDataProvider
{
    IEnumerable<DataLocalizedString> GetDescriptors();
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Localization.Data;

public interface ILocalizationDataProvider
{
    Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync();
}

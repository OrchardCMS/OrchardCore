using System.Collections.Generic;

namespace OrchardCore.Localization.Data;

public interface IDataResourceStringProvider
{
    IEnumerable<CultureDictionaryRecordKey> GetAllResourceStrings();
}

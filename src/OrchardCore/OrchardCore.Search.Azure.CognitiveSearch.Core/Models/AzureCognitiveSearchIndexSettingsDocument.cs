using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class AzureCognitiveSearchIndexSettingsDocument : Document
{
    public Dictionary<string, AzureCognitiveSearchIndexSettings> IndexSettings { get; set; } = [];
}

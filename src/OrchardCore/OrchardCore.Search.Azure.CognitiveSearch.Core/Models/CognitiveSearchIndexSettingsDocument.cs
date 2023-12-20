using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class CognitiveSearchIndexSettingsDocument : Document
{
    public Dictionary<string, CognitiveSearchIndexSettings> IndexSettings { get; set; } = [];
}

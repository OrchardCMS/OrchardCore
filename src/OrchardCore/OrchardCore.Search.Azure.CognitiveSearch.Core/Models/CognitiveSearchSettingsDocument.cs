using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Azure.CognitiveSearch.Models;

public class CognitiveSearchSettingsDocument : Document
{
    public Dictionary<string, CognitiveSearchSettings> IndexSettings { get; set; } = [];
}

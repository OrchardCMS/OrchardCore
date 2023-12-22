using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.AzureAI.Models;

public class AzureAIIndexSettingsDocument : Document
{
    public Dictionary<string, AzureAIIndexSettings> IndexSettings { get; set; } = [];
}

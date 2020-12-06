using System.Collections.Generic;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.Data.Documents;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationDocument : Document
    {
        public Dictionary<string, LocalizationEntry> Localizations { get; set; } = new Dictionary<string, LocalizationEntry>();
        public Dictionary<string, List<LocalizationEntry>> LocalizationSets { get; set; } = new Dictionary<string, List<LocalizationEntry>>();
    }
}

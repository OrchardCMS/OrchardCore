using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Localization.Dynamic.Models
{
    public class TranslationsDocument : Document
    {
        public Dictionary<string, Translation> Translations { get; set; } = new Dictionary<string, Translation>(StringComparer.OrdinalIgnoreCase);
    }
}

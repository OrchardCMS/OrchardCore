using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.DataLocalization.Models;

public class TranslationsDocument : Document
{
    public TranslationsDocument()
    {
        Translations = new Dictionary<string, IEnumerable<Translation>>(StringComparer.OrdinalIgnoreCase);
    }

    public Dictionary<string, IEnumerable<Translation>> Translations { get; }
}

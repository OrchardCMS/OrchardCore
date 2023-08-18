using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Secrets.Models
{
    public class SecretBindingsDocument : Document
    {
        public Dictionary<string, SecretBinding> SecretBindings { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}

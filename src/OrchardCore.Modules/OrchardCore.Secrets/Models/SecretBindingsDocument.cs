using System;
using System.Collections.Generic;

namespace OrchardCore.Secrets.Models
{
    public class SecretBindingsDocument
    {
        public IDictionary<string, SecretBinding> SecretBindings { get; } = new Dictionary<string, SecretBinding>(StringComparer.OrdinalIgnoreCase);
    }
}

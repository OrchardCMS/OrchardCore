using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Secrets.Models;

public class SecretsDocument : Document
{
    public Dictionary<string, string> Secrets { get; } = new(StringComparer.OrdinalIgnoreCase);
}

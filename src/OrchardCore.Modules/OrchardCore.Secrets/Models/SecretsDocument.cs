using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Secrets.Models;

public class SecretsDocument : Document
{
    public Dictionary<string, SecretDocument> Secrets { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public class SecretDocument
{
    public string Value { get; set; }
}

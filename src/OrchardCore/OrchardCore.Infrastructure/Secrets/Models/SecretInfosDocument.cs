using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Secrets.Models;

public class SecretInfosDocument : Document
{
    public Dictionary<string, SecretInfo> SecretInfos { get; } = new(StringComparer.OrdinalIgnoreCase);
}

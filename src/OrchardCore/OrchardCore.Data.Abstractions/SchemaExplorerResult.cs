using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Data.Migration;

public class SchemaExplorerResult
{
    public string TenantName { get; set; }
    public string TablePrefix { get; set; }
    public IEnumerable<string> TableNames { get; set; } = Enumerable.Empty<string>();
    public bool Success => Error == null;
    public Exception Error { get; set; }
    public string Message { get; set; }
}

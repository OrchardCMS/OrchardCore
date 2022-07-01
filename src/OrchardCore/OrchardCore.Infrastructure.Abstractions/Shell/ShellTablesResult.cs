using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Shell;

public class ShellTablesResult
{
    public string TenantName { get; set; }
    public string TablePrefix { get; set; }
    public IEnumerable<string> TableNames { get; set; } = Enumerable.Empty<string>();
    public bool Success => Error == null;
    public string Message { get; set; }
    public Exception Error { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Shell;

public class ShellDbTablesResult
{
    public string TenantName { get; set; } = String.Empty;
    public string TablePrefix { get; set; } = String.Empty;
    public IEnumerable<string> TableNames { get; set; } = Enumerable.Empty<string>();
    public bool Success => Error == null;

    private string _message;
    public string Message
    {
        get => _message ?? Error?.Message ?? String.Empty;
        set => _message = value;
    }

    public Exception Error { get; set; }
}

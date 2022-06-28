using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Data.Migration;

public class DataMigrationAnalyzerResult
{
    public string TenantName { get; set; }
    public string TablePrefix { get; set; }
    public IEnumerable<string> Tables { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> IndexTables { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> BridgeTables { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> DocumentTables { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Collections { get; set; } = Enumerable.Empty<string>();
    public bool Success => Error == null;
    public Exception Error { get; set; }
    public string Message { get; set; }
}

using System;

namespace OrchardCore.ContentsTransfer;

public class ImportColumn
{
    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsRequired { get; set; }

    public string[] AdditionalNames { get; set; } = Array.Empty<string>();

    public string[] ValidValues { get; set; } = Array.Empty<string>();

    public ImportColumnType Type { get; set; }
}

public enum ImportColumnType
{
    All,
    ImportOnly,
    ExportOnly,
}

using System;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public DatabaseTableOptions()
    {
    }

    public DatabaseTableOptions(ShellSettings shellSettings)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        DocumentTable = shellSettings.GetDocumentTableOrDefault();
        TableNameSeparator= shellSettings.GetTableNameSeparatorOrDefault();
        IdentityColumnSize = shellSettings.GetIdentityColumnSizeOrDefault();
    }

    public virtual string DocumentTable { get; }

    public virtual string TableNameSeparator { get; }

    public virtual string IdentityColumnSize { get; }
}

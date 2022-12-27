using System;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public DatabaseTableOptions(ShellSettings shellSettings)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        DocumentTable = shellSettings.GetDocumentTableName();
        TableNameSeparator= shellSettings.GetTableNameSeparator();
        IdentityColumnSize = shellSettings.GetIdentityColumnSize();
    }

    public virtual string DocumentTable { get; }

    public virtual string TableNameSeparator { get; }

    public virtual string IdentityColumnSize { get; }
}

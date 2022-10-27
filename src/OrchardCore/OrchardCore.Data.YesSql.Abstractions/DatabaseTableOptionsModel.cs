using System;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class DatabaseTableOptionsModel : DatabaseTableOptions
{
    public DatabaseTableOptionsModel(ShellSettings shellSettings) : base(shellSettings) { }

    public DatabaseTableOptionsModel(
        string schema,
        string documentTable,
        string tableNameSeparator,
        IdentityColumnSize identityColumnSize)
            : base(schema, documentTable, tableNameSeparator, identityColumnSize) { }

    public override string TableNameSeparator =>
        !String.IsNullOrEmpty(base.TableNameSeparator)
        ? base.TableNameSeparator :
        "NULL";
}

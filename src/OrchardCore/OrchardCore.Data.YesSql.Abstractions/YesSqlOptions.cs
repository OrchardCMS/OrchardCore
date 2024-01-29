using YesSql;

namespace OrchardCore.Data.YesSql;

public class YesSqlOptions
{
    public int CommandsPageSize { get; set; } = 500;

    public bool QueryGatingEnabled { get; set; } = true;

    public IIdGenerator IdGenerator { get; set; }

    public IAccessorFactory IdentifierAccessorFactory { get; set; }

    public IAccessorFactory VersionAccessorFactory { get; set; }

    public IContentSerializer ContentSerializer { get; set; }
}

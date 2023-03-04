using System.Collections.Concurrent;
using OrchardCore.Testing.Apis.Security;

namespace OrchardCore.Testing.Apis;

public class SiteContextOptions
{
    static SiteContextOptions()
    {
        PermissionsContexts = new();
    }

    public static ConcurrentDictionary<string, PermissionsContext> PermissionsContexts { get; set; }

    public string RecipeName { get; set; } = "Blog";

    public string DatabaseProvider { get; set; } = "Sqlite";

    public string ConnectionString { get; set; }

    public string TablePrefix { get; set; }

    public PermissionsContext PermissionsContext { get; set; }
}

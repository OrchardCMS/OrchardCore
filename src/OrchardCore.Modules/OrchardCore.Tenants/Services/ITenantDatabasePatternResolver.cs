using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tenants.Services;

public interface ITenantDatabasePatternResolver
{
    TenantDatabasePatternResolution Resolve(ShellSettings shellSettings);

    TenantDatabasePatternResolution Apply(TenantModelBase model);
}

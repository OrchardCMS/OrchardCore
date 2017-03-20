using OrchardCore.Tenant.Builders.Models;
using System;

namespace OrchardCore.Tenant.Builders
{
    public interface ITenantContainerFactory
    {
        IServiceProvider CreateContainer(TenantSettings settings, TenantBlueprint blueprint);
    }
}
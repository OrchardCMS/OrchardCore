using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Tenants.Models;

namespace OrchardCore.Tenants.Services;

public interface ITenantValidator
{
    Task<IEnumerable<ModelError>> ValidateAsync(TenantModelBase model);
}

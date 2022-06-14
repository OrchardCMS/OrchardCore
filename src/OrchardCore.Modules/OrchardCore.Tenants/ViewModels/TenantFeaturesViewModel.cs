using System.Collections.Generic;
using OrchardCore.Features.Models;

namespace OrchardCore.Tenants.ViewModels;

public class TenantFeaturesViewModel
{
    public string Tenant { get; set; }
    public IEnumerable<ModuleFeature> Features { get; set; }
}


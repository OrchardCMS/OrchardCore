using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Data;

namespace OrchardCore.Tenants.Models;

public abstract class TenantModelBase : IDbConnectionInfo
{
    public string Description { get; set; }

    [Required]
    public string Name { get; set; }

    public string Category { get; set; }

    public string DatabaseProvider { get; set; }

    public string RequestUrlPrefix { get; set; }

    public string RequestUrlHost { get; set; }

    public string ConnectionString { get; set; }

    public string TablePrefix { get; set; }

    public string Schema { get; set; }

    public string RecipeName { get; set; }

    public string[] FeatureProfiles { get; set; }

    [BindNever]
    public bool IsNewTenant { get; set; }
}

using Microsoft.Extensions.Options;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.Tenants.AuditTrail;

/// <summary>
/// Describes the audit trail events of the remote management of a tenant, which grants remote access to that tenant.
/// </summary>
public sealed class TenantRemoteManagementAuditTrailEventConfiguration : IConfigureOptions<AuditTrailOptions>
{
    /// <summary>
    /// The name of the audit trail category.
    /// </summary>
    public const string CategoryName = "TenantRemoteManagement";

    /// <summary>
    /// The name of the event recorded when remote management is enabled for a tenant, with or without an administrative application.
    /// </summary>
    public const string Enabled = nameof(Enabled);

    /// <summary>
    /// The name of the event recorded when remote management is disabled for a tenant.
    /// </summary>
    public const string Disabled = nameof(Disabled);

    /// <inheritdoc />
    public void Configure(AuditTrailOptions options)
    {
        options.For<TenantRemoteManagementAuditTrailEventConfiguration>(CategoryName, S => S["Tenant remote management"])
            .WithEvent(Enabled, S => S["Enabled"], S => S["Remote management was enabled for a tenant."], true)
            .WithEvent(Disabled, S => S["Disabled"], S => S["Remote management was disabled for a tenant."], true);
    }
}

/// <summary>
/// The data of an audit trail event of the remote management of a tenant. It never holds a secret.
/// </summary>
public sealed class TenantRemoteManagementAuditEvent
{
    /// <summary>
    /// Gets or sets the name of the tenant.
    /// </summary>
    public string TenantName { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the administrative application created for the tenant, if any.
    /// </summary>
    public string ClientId { get; set; }
}

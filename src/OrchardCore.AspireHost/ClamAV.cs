using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace OrchardCore.AspireHost;

public sealed class ClamAVResource : ContainerResource, IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "tcp";

    private EndpointReference _primaryEndpoint;

    public ClamAVResource(string name)
        : base(name)
    {
    }

    public EndpointReference PrimaryEndpoint
        => _primaryEndpoint ??= new EndpointReference(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression
        => ReferenceExpression.Create(
            $"tcp://{PrimaryEndpoint.Property(EndpointProperty.Host)}:{PrimaryEndpoint.Property(EndpointProperty.Port)}");
}

internal static class ClamAVResourceBuilderExtensions
{
    public static IResourceBuilder<ClamAVResource> AddClamAV(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null)
    {
        var resource = new ClamAVResource(name);

        return builder.AddResource(resource)
            .WithImage("clamav/clamav")
            .WithImageRegistry("docker.io")
            .WithImageTag("latest")
            .WithEnvironment("CLAMAV_NO_FRESHCLAMD", "true")
            .WithEndpoint(port: port, name: ClamAVResource.PrimaryEndpointName, targetPort: 3310);
    }

    public static IResourceBuilder<ClamAVResource> WithDataVolume(
        this IResourceBuilder<ClamAVResource> builder,
        string name,
        bool isReadOnly = false) =>
        builder.WithVolume(name, "/var/lib/clamav", isReadOnly);

    public static IResourceBuilder<ClamAVResource> WithDataBindMount(
        this IResourceBuilder<ClamAVResource> builder,
        string source,
        bool isReadOnly = false) =>
        builder.WithBindMount(source, "/var/lib/clamav", isReadOnly);
}

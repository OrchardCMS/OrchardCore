namespace OrchardCore.OpenApi;

/// <summary>
/// Marker registered by each API documentation UI's own feature-scoped <c>Startup</c>
/// (<c>SwaggerUIStartup</c>, <c>ReDocUIStartup</c>, <c>ScalarUIStartup</c>, in the module project).
/// Its mere presence in <c>IEnumerable&lt;IOpenApiUIFeature&gt;</c> means the owning feature is
/// enabled for the current tenant, since OrchardCore only runs a feature's <c>ConfigureServices</c>
/// when that feature is enabled — so consumers never need to compare feature IDs against
/// <see cref="Environment.Shell.IShellFeaturesManager"/> by hand.
/// </summary>
public interface IOpenApiUIFeature;

public sealed class SwaggerUIFeature : IOpenApiUIFeature;

public sealed class ReDocUIFeature : IOpenApiUIFeature;

public sealed class ScalarUIFeature : IOpenApiUIFeature;

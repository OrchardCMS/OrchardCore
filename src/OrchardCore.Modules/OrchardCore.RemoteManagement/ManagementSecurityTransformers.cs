using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Declares how the operations of the OpenAPI document authenticate. The authority comes from the
/// management manifest, as it depends on the tenant and the request, so this is not an OAuth 2.0 scheme.
/// </summary>
internal sealed class ManagementSecurityDocumentTransformer : IOpenApiDocumentTransformer
{
    internal const string SchemeName = "Bearer";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            Description = $"An access token from the OpenID Connect server of the tenant, with the '{RemoteManagementConstants.ManagementScope}' scope. The bootstrap document and the management manifest describe the authority and the grants.",
        };

        return Task.CompletedTask;
    }
}

/// <summary>
/// Marks the operations authenticating with the API scheme as using the scheme of the document. The
/// operations of the CLI allowing anonymous access declare it as optional, as a token can be sent to them.
/// </summary>
internal sealed class ManagementSecurityOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (RequiresBearerToken(context.Description.ActionDescriptor.EndpointMetadata))
        {
            operation.Security ??= [];
            operation.Security.Add(CreateRequirement(context.Document));
        }
        else if (AllowsOptionalBearerToken(context.Description.ActionDescriptor.EndpointMetadata))
        {
            // An empty requirement is how OpenAPI tells that the operation can be called without credentials.
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement());
            operation.Security.Add(CreateRequirement(context.Document));
        }

        return Task.CompletedTask;
    }

    private static OpenApiSecurityRequirement CreateRequirement(OpenApiDocument document) => new()
    {
        [new OpenApiSecuritySchemeReference(ManagementSecurityDocumentTransformer.SchemeName, document)] = [],
    };

    internal static bool AllowsOptionalBearerToken(IEnumerable<object> metadata)
    {
        var items = metadata as IReadOnlyCollection<object> ?? metadata.ToArray();

        return items.OfType<IAllowAnonymous>().Any() && items.OfType<CliOperationMetadata>().Any();
    }

    internal static bool RequiresBearerToken(IEnumerable<object> metadata)
    {
        var items = metadata as IReadOnlyCollection<object> ?? metadata.ToArray();

        return !items.OfType<IAllowAnonymous>().Any() &&
            (items.OfType<AuthorizationPolicy>().Any(policy => policy.AuthenticationSchemes.Contains(OrchardCoreConstants.AuthenticationSchemes.Api)) ||
            items.OfType<IAuthorizeData>().Any(data => data.AuthenticationSchemes?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(OrchardCoreConstants.AuthenticationSchemes.Api) == true));
    }
}

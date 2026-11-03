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
/// Marks the operations authenticating with the API scheme as using the scheme of the document.
/// </summary>
internal sealed class ManagementSecurityOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (RequiresBearerToken(context.Description.ActionDescriptor.EndpointMetadata))
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(ManagementSecurityDocumentTransformer.SchemeName, context.Document)] = [],
            });
        }

        return Task.CompletedTask;
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

using Microsoft.AspNetCore.Builder;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Provides endpoint conventions for remote management operations.
/// </summary>
public static class EndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds CLI projection metadata to an API endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="metadata">The CLI operation metadata.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static TBuilder WithCliCommand<TBuilder>(this TBuilder builder, CliOperationMetadata metadata)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(metadata);

        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(metadata));

        return builder;
    }
}

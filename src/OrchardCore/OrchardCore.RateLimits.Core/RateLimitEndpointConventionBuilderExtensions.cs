using Microsoft.AspNetCore.Builder;

namespace OrchardCore.RateLimits;

/// <summary>
/// Adds soft rate-limit group metadata to endpoint builders.
/// </summary>
public static class RateLimitEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds a single rate-limit group to the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="groupName">The group name to add.</param>
    /// <returns>The updated endpoint builder.</returns>
    public static TBuilder WithRateLimitGroup<TBuilder>(this TBuilder builder, string groupName)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithRateLimitGroups(groupName);

    /// <summary>
    /// Adds one or more rate-limit groups to the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint builder type.</typeparam>
    /// <param name="builder">The endpoint builder.</param>
    /// <param name="groupNames">The group names to add.</param>
    /// <returns>The updated endpoint builder.</returns>
    public static TBuilder WithRateLimitGroups<TBuilder>(this TBuilder builder, params string[] groupNames)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(new RateLimitGroupAttribute(groupNames)));

        return builder;
    }
}

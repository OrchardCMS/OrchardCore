namespace OrchardCore.RateLimits;

/// <summary>
/// Assigns one or more soft rate-limit groups to a controller action, page handler, or endpoint metadata collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RateLimitGroupAttribute : Attribute, IRateLimitGroupMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitGroupAttribute"/> class.
    /// </summary>
    /// <param name="groupName">The rate-limit group name to assign.</param>
    public RateLimitGroupAttribute(string groupName)
        : this([groupName])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitGroupAttribute"/> class.
    /// </summary>
    /// <param name="groupNames">The rate-limit group names to assign.</param>
    public RateLimitGroupAttribute(params string[] groupNames)
    {
        ArgumentNullException.ThrowIfNull(groupNames);

        GroupNames = [.. groupNames
            .Select(static groupName => groupName?.Trim())
            .Where(static groupName => !string.IsNullOrWhiteSpace(groupName))
            .Distinct(StringComparer.OrdinalIgnoreCase)];

        if (GroupNames.Count == 0)
        {
            throw new ArgumentException("At least one rate-limit group name is required.", nameof(groupNames));
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GroupNames { get; }
}

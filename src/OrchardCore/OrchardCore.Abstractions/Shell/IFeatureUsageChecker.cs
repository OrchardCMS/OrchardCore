using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Defines a contract for determining whether a specified feature is currently in use.
/// </summary>
public interface IFeatureUsageChecker
{
    /// <summary>
    /// Determines whether the specified feature is disabled and currently in use.
    /// </summary>
    /// <param name="feature">The feature to check for active usage. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
    /// feature is disabled and in use; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsDisabledFeatureInUseAsync(IFeatureInfo feature);
}

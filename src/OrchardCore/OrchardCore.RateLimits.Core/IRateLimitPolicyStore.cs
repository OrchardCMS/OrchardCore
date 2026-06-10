using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Core;

/// <summary>
/// Provides persistence operations for draft and published rate-limit policies.
/// </summary>
public interface IRateLimitPolicyStore
{
    /// <summary>
    /// Retrieves the specified version of a policy.
    /// </summary>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="version">The policy version to retrieve.</param>
    /// <returns>The requested policy version, or <c>null</c> when it does not exist.</returns>
    ValueTask<RateLimitPolicy> FindByIdAsync(string policyId, PolicyVersion version);

    /// <summary>
    /// Retrieves all stored policies for the requested version.
    /// </summary>
    /// <param name="version">The policy version to retrieve.</param>
    /// <returns>A sequence of stored policies for the requested version.</returns>
    ValueTask<IReadOnlyCollection<RateLimitPolicy>> GetAllAsync(PolicyVersion version);

    /// <summary>
    /// Creates a new draft policy.
    /// </summary>
    /// <param name="policy">The policy to create.</param>
    ValueTask CreateAsync(RateLimitPolicy policy);

    /// <summary>
    /// Updates the draft version of an existing policy.
    /// </summary>
    /// <param name="policy">The policy to update.</param>
    ValueTask UpdateAsync(RateLimitPolicy policy);

    /// <summary>
    /// Deletes the draft and published versions of the specified policy.
    /// </summary>
    /// <param name="policy">The policy to delete.</param>
    /// <returns><c>true</c> when a policy was deleted; otherwise <c>false</c>.</returns>
    ValueTask<bool> DeleteAsync(RateLimitPolicy policy);

    /// <summary>
    /// Publishes the specified draft policies and returns the resulting published policies.
    /// </summary>
    /// <param name="policies">The draft policies to publish.</param>
    /// <returns>The published policies created by the operation.</returns>
    ValueTask<IReadOnlyCollection<RateLimitPolicy>> PublishAsync(IEnumerable<RateLimitPolicy> policies);

    /// <summary>
    /// Unpublishes the specified policy while preserving an editable draft.
    /// </summary>
    /// <param name="policy">The policy to unpublish.</param>
    /// <returns><c>true</c> when the policy was unpublished; otherwise <c>false</c>.</returns>
    ValueTask<bool> UnpublishAsync(RateLimitPolicy policy);
}

using OrchardCore.Documents;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Stores rate-limit policies in the tenant document store.
/// </summary>
public sealed class RateLimitPolicyStore : IRateLimitPolicyStore
{
    private readonly IDocumentManager<RateLimitPolicyDocument> _documentManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitPolicyStore"/> class.
    /// </summary>
    /// <param name="documentManager">The document manager used to persist policy documents.</param>
    public RateLimitPolicyStore(IDocumentManager<RateLimitPolicyDocument> documentManager)
    {
        _documentManager = documentManager;
    }

    /// <inheritdoc />
    public async ValueTask<RateLimitPolicy> FindByIdAsync(string policyId, PolicyVersion version)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return version switch
        {
            PolicyVersion.Current => FindCurrent(document, policyId),
            PolicyVersion.Enabled => FindEnabled(document, policyId),
            _ => null,
        };
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyCollection<RateLimitPolicy>> GetAllAsync(PolicyVersion version)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return version switch
        {
            PolicyVersion.Current => [.. GetCurrentPolicies(document).Values],
            PolicyVersion.Enabled => [.. GetEnabledPolicies(document).Values],
            _ => [],
        };
    }

    /// <inheritdoc />
    public async ValueTask CreateAsync(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy.PolicyId ??= IdGenerator.GenerateId();

        var document = await _documentManager.GetOrCreateMutableAsync();
        Upsert(document.Policies, CloneForStorage(policy));

        await _documentManager.UpdateAsync(document);
    }

    /// <inheritdoc />
    public async ValueTask UpdateAsync(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentException.ThrowIfNullOrEmpty(policy.PolicyId);

        var document = await _documentManager.GetOrCreateMutableAsync();
        Upsert(document.Policies, CloneForStorage(policy));

        await _documentManager.UpdateAsync(document);
    }

    /// <inheritdoc />
    public async ValueTask<bool> DeleteAsync(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentException.ThrowIfNullOrEmpty(policy.PolicyId);

        var policyId = policy.PolicyId;
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        var document = await _documentManager.GetOrCreateMutableAsync();
        var removed = document.Policies.RemoveAll(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal)) > 0;

        await _documentManager.UpdateAsync(document);

        return removed;
    }

    /// <inheritdoc />
    public async ValueTask<bool> SetStatusAsync(string policyId, bool isEnabled)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        var document = await _documentManager.GetOrCreateMutableAsync();
        var policy = FindCurrent(document, policyId);
        if (policy is null)
        {
            return false;
        }

        policy.IsEnabled = isEnabled;
        policy.EnabledUtc = isEnabled ? DateTime.UtcNow : null;
        Upsert(document.Policies, CloneForStorage(policy));
        await _documentManager.UpdateAsync(document);

        return true;
    }

    /// <summary>
    /// Finds the current policy with the specified identifier in the provided document.
    /// </summary>
    /// <param name="document">The policy document to search.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <returns>The matching current policy, or <c>null</c> when none exists.</returns>
    public static RateLimitPolicy FindCurrent(RateLimitPolicyDocument document, string policyId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        return document.Policies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal)) is { } policy
            ? CreateCurrentPolicy(policy)
            : null;
    }

    /// <summary>
    /// Finds the enabled policy with the specified identifier in the provided document.
    /// </summary>
    /// <param name="document">The policy document to search.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <returns>The matching enabled policy, or <c>null</c> when none exists.</returns>
    public static RateLimitPolicy FindEnabled(RateLimitPolicyDocument document, string policyId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        return document.Policies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal)) is { IsEnabled: true } policy
            ? CreateCurrentPolicy(policy)
            : null;
    }

    /// <summary>
    /// Enumerates the current and enabled policy pairs contained in the specified document.
    /// </summary>
    /// <param name="document">The policy document to inspect.</param>
    /// <returns>The policy identifier with its current and enabled snapshots.</returns>
    public static IEnumerable<(string PolicyId, RateLimitPolicy Current, RateLimitPolicy Enabled)> GetPolicies(RateLimitPolicyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return GetCurrentPolicies(document).Keys
            .Concat(GetEnabledPolicies(document).Keys)
            .Distinct(StringComparer.Ordinal)
            .Select(policyId => (policyId, FindCurrent(document, policyId), FindEnabled(document, policyId)));
    }

    /// <summary>
    /// Computes the status of a policy from its enabled state.
    /// </summary>
    /// <param name="isEnabled"><c>true</c> when the policy is enabled.</param>
    /// <returns>The computed policy status.</returns>
    public static RateLimitPolicyStatus GetStatus(bool isEnabled)
        => isEnabled ? RateLimitPolicyStatus.Enabled : RateLimitPolicyStatus.Disabled;

    /// <summary>
    /// Creates a deep clone of the specified policy.
    /// </summary>
    /// <param name="policy">The policy to clone.</param>
    /// <returns>A cloned policy instance, or <c>null</c> when <paramref name="policy"/> is <c>null</c>.</returns>
    public static RateLimitPolicy Clone(RateLimitPolicy policy)
    {
        if (policy is null)
        {
            return null;
        }

        return new RateLimitPolicy
        {
            PolicyId = policy.PolicyId,
            Name = policy.Name,
            Description = policy.Description,
            OwnerId = policy.OwnerId,
            Author = policy.Author,
            Scope = policy.Scope,
            Path = policy.Path,
            GroupName = policy.GroupName,
            IsEnabled = policy.IsEnabled,
            EnabledUtc = policy.EnabledUtc,
            Status = policy.Status,
            Limiters = [.. policy.Limiters.Select(Clone)],
        };
    }

    private static Dictionary<string, RateLimitPolicy> GetCurrentPolicies(RateLimitPolicyDocument document)
    {
        return document.Policies
            .Select(CreateCurrentPolicy)
            .Where(static policy => policy is not null)
            .ToDictionary(x => x.PolicyId, StringComparer.Ordinal);
    }

    private static Dictionary<string, RateLimitPolicy> GetEnabledPolicies(RateLimitPolicyDocument document)
    {
        return document.Policies
            .Where(static policy => policy is not null && policy.IsEnabled)
            .Select(CreateCurrentPolicy)
            .Where(static policy => policy is not null)
            .ToDictionary(x => x.PolicyId, StringComparer.Ordinal);
    }

    private static RateLimitPolicy CreateCurrentPolicy(RateLimitPolicy policy)
    {
        if (policy is null)
        {
            return null;
        }

        var clonedPolicy = Clone(policy);
        clonedPolicy.Status = GetStatus(clonedPolicy.IsEnabled);

        return clonedPolicy;
    }

    private static RateLimitPolicy CloneForStorage(RateLimitPolicy policy)
    {
        var clonedPolicy = Clone(policy);
        clonedPolicy.Status = default;
        clonedPolicy.EnabledUtc = clonedPolicy.IsEnabled
            ? clonedPolicy.EnabledUtc ?? DateTime.UtcNow
            : null;

        return clonedPolicy;
    }

    private static RateLimitLimiter Clone(RateLimitLimiter limiter)
    {
        return new RateLimitLimiter
        {
            Id = limiter.Id,
            Source = limiter.Source,
            Properties = limiter.Properties?.DeepClone()?.AsObject() ?? [],
        };
    }

    private static void Upsert(List<RateLimitPolicy> policies, RateLimitPolicy policy)
    {
        var index = policies.FindIndex(x => string.Equals(x.PolicyId, policy.PolicyId, StringComparison.Ordinal));
        if (index == -1)
        {
            policies.Add(policy);
            return;
        }

        policies[index] = policy;
    }
}

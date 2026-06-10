using System.Text.Json;
using OrchardCore.Documents;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Stores draft and published rate-limit policies in the tenant document store.
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
            PolicyVersion.Draft => CreateDraftPolicy(FindDraft(document, policyId), FindPublished(document, policyId)),
            PolicyVersion.Published => CreatePublishedPolicy(FindPublished(document, policyId)),
            _ => null,
        };
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyCollection<RateLimitPolicy>> GetAllAsync(PolicyVersion version)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return version switch
        {
            PolicyVersion.Draft => document.DraftPolicies
                .Select(draft => CreateDraftPolicy(draft, FindPublished(document, draft.PolicyId)))
                .ToArray(),
            PolicyVersion.Published => document.PublishedPolicies
                .Select(CreatePublishedPolicy)
                .ToArray(),
            _ => [],
        };
    }

    /// <inheritdoc />
    public async ValueTask CreateAsync(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy.PolicyId ??= IdGenerator.GenerateId();

        var document = await _documentManager.GetOrCreateMutableAsync();
        var clonedDraft = Clone(policy);
        clonedDraft.PublishedUtc = null;

        Upsert(document.DraftPolicies, clonedDraft);

        await _documentManager.UpdateAsync(document);
    }

    /// <inheritdoc />
    public async ValueTask UpdateAsync(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentException.ThrowIfNullOrEmpty(policy.PolicyId);

        var document = await _documentManager.GetOrCreateMutableAsync();
        var clonedDraft = Clone(policy);
        clonedDraft.PublishedUtc = null;

        Upsert(document.DraftPolicies, clonedDraft);

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
        var removed = document.DraftPolicies.RemoveAll(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal)) > 0;

        removed |= document.PublishedPolicies.RemoveAll(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal)) > 0;

        await _documentManager.UpdateAsync(document);

        return removed;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyCollection<RateLimitPolicy>> PublishAsync(IEnumerable<RateLimitPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var ids = policies
            .Where(static policy => policy is not null)
            .Select(static policy => policy.PolicyId)
            .Where(static policyId => !string.IsNullOrWhiteSpace(policyId))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        if (ids.Count == 0)
        {
            return [];
        }

        var document = await _documentManager.GetOrCreateMutableAsync();
        var publishedUtc = DateTime.UtcNow;
        var publishedPolicies = new List<RateLimitPolicy>();

        foreach (var draft in document.DraftPolicies.Where(x => ids.Contains(x.PolicyId)).ToArray())
        {
            var publishedPolicy = Clone(draft);
            publishedPolicy.PublishedUtc = publishedUtc;

            Upsert(document.PublishedPolicies, publishedPolicy);
            publishedPolicies.Add(Clone(publishedPolicy));
        }

        document.DraftPolicies.RemoveAll(x => ids.Contains(x.PolicyId));

        await _documentManager.UpdateAsync(document);

        return publishedPolicies;
    }

    /// <summary>
    /// Finds the draft policy with the specified identifier in the provided document.
    /// </summary>
    /// <param name="document">The policy document to search.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <returns>The matching draft policy, or <c>null</c> when none exists.</returns>
    public static RateLimitPolicy FindDraft(RateLimitPolicyDocument document, string policyId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        return document.DraftPolicies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Finds the published policy with the specified identifier in the provided document.
    /// </summary>
    /// <param name="document">The policy document to search.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <returns>The matching published policy, or <c>null</c> when none exists.</returns>
    public static RateLimitPolicy FindPublished(RateLimitPolicyDocument document, string policyId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        return document.PublishedPolicies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Enumerates the draft and published policy pairs contained in the specified document.
    /// </summary>
    /// <param name="document">The policy document to inspect.</param>
    /// <returns>The policy identifier with its draft and published snapshots.</returns>
    public static IEnumerable<(string PolicyId, RateLimitPolicy Draft, RateLimitPolicy Published)> GetPolicies(RateLimitPolicyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.DraftPolicies.Select(static x => x.PolicyId)
            .Concat(document.PublishedPolicies.Select(static x => x.PolicyId))
            .Distinct(StringComparer.Ordinal)
            .Select(policyId => (policyId, FindDraft(document, policyId), FindPublished(document, policyId)));
    }

    /// <summary>
    /// Computes the status of a policy from its draft and published snapshots.
    /// </summary>
    /// <param name="draft">The draft policy.</param>
    /// <param name="published">The published policy.</param>
    /// <returns>The computed policy status.</returns>
    public static RateLimitPolicyStatus GetStatus(RateLimitPolicy draft, RateLimitPolicy published)
    {
        if (published is null)
        {
            return RateLimitPolicyStatus.Draft;
        }

        if (draft is null)
        {
            return RateLimitPolicyStatus.Published;
        }

        return AreEquivalent(draft, published)
            ? RateLimitPolicyStatus.Published
            : RateLimitPolicyStatus.PublishedWithDraft;
    }

    /// <summary>
    /// Determines whether two policies are equivalent for publish-status comparisons.
    /// </summary>
    /// <param name="left">The first policy.</param>
    /// <param name="right">The second policy.</param>
    /// <returns><c>true</c> if the policies are equivalent; otherwise <c>false</c>.</returns>
    public static bool AreEquivalent(RateLimitPolicy left, RateLimitPolicy right)
    {
        if (left is null || right is null)
        {
            return left == right;
        }

        return JsonSerializer.Serialize(left, JOptions.Default) == JsonSerializer.Serialize(right, JOptions.Default);
    }

    private static RateLimitPolicy CreateDraftPolicy(RateLimitPolicy draft, RateLimitPolicy published)
    {
        if (draft is null)
        {
            return null;
        }

        var currentPolicy = Clone(draft);
        currentPolicy.Status = GetStatus(draft, published);
        currentPolicy.PublishedUtc = published?.PublishedUtc ?? draft.PublishedUtc;

        return currentPolicy;
    }

    private static RateLimitPolicy CreatePublishedPolicy(RateLimitPolicy published)
    {
        if (published is null)
        {
            return null;
        }

        var clonedPolicy = Clone(published);
        clonedPolicy.Status = RateLimitPolicyStatus.Published;
        return clonedPolicy;
    }

    private static RateLimitPolicy Clone(RateLimitPolicy policy)
    {
        return new RateLimitPolicy
        {
            PolicyId = policy.PolicyId,
            Name = policy.Name,
            Description = policy.Description,
            OwnerId = policy.OwnerId,
            Author = policy.Author,
            Scope = policy.Scope,
            Path = policy.Path,
            PublishedUtc = policy.PublishedUtc,
            Status = policy.Status,
            Limiters = [.. policy.Limiters.Select(Clone)],
        };
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

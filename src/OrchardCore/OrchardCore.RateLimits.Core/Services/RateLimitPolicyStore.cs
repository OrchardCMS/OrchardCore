using System.Text.Json;
using OrchardCore.Documents;
using OrchardCore.RateLimits.Core;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

public sealed class RateLimitPolicyStore : IRateLimitPolicyStore
{
    private readonly IDocumentManager<RateLimitPolicyDocument> _documentManager;

    public RateLimitPolicyStore(IDocumentManager<RateLimitPolicyDocument> documentManager)
    {
        _documentManager = documentManager;
    }

    public Task<RateLimitPolicyDocument> LoadAsync()
        => _documentManager.GetOrCreateMutableAsync();

    public Task<RateLimitPolicyDocument> GetAsync()
        => _documentManager.GetOrCreateImmutableAsync();

    public Task SaveAsync(RateLimitPolicyDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return _documentManager.UpdateAsync(document);
    }

    public async Task<IEnumerable<RateLimitPolicy>> GetPublishedPoliciesAsync()
        => (await GetAsync()).Policies
            .Where(static entry => entry.Published is not null)
            .Select(static entry => entry.Published);

    public async Task<RateLimitPolicyEntry> FindAsync(string policyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        return (await GetAsync()).Policies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal));
    }

    public async Task SaveDraftAsync(string policyId, RateLimitPolicy draft)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId);
        ArgumentNullException.ThrowIfNull(draft);

        var document = await LoadAsync();
        var entry = document.Policies.FirstOrDefault(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal));

        if (entry is null)
        {
            entry = new RateLimitPolicyEntry
            {
                PolicyId = policyId,
            };

            document.Policies.Add(entry);
        }

        entry.Draft = Clone(draft);

        await _documentManager.UpdateAsync(document);
    }

    public async Task DeleteAsync(string policyId)
    {
        ArgumentException.ThrowIfNullOrEmpty(policyId);

        var document = await LoadAsync();
        document.Policies.RemoveAll(x => string.Equals(x.PolicyId, policyId, StringComparison.Ordinal));
        await _documentManager.UpdateAsync(document);
    }

    public async Task PublishAsync(IEnumerable<string> policyIds)
    {
        ArgumentNullException.ThrowIfNull(policyIds);

        var ids = policyIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        if (ids.Count == 0)
        {
            return;
        }

        var document = await LoadAsync();
        var publishedUtc = DateTime.UtcNow;

        foreach (var entry in document.Policies.Where(x => ids.Contains(x.PolicyId) && x.Draft is not null))
        {
            entry.Published = Clone(entry.Draft);
            entry.PublishedUtc = publishedUtc;
        }

        await _documentManager.UpdateAsync(document);
    }

    public static RateLimitPolicyEntry Clone(RateLimitPolicyEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        return new RateLimitPolicyEntry
        {
            PolicyId = entry.PolicyId,
            Draft = entry.Draft is null
                ? null
                : Clone(entry.Draft),
            Published = entry.Published is null
                ? null
                : Clone(entry.Published),
            PublishedUtc = entry.PublishedUtc,
        };
    }

    public static RateLimitPolicy Clone(RateLimitPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new RateLimitPolicy
        {
            Name = policy.Name,
            Description = policy.Description,
            OwnerId = policy.OwnerId,
            Author = policy.Author,
            Scope = policy.Scope,
            RouteName = policy.RouteName,
            Path = policy.Path,
            Limiters =
            [
                .. policy.Limiters.Select(Clone),
            ],
        };
    }

    public static RateLimitLimiter Clone(RateLimitLimiter limiter)
    {
        ArgumentNullException.ThrowIfNull(limiter);

        return new RateLimitLimiter
        {
            Id = limiter.Id,
            Source = limiter.Source,
            Properties = limiter.Properties?.DeepClone()?.AsObject() ?? [],
        };
    }

    public static RateLimitPolicyEntry CreateEntry(RateLimitPolicy policy, bool publish)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var entry = new RateLimitPolicyEntry
        {
            PolicyId = IdGenerator.GenerateId(),
            Draft = Clone(policy),
        };

        if (publish)
        {
            entry.Published = Clone(policy);
            entry.PublishedUtc = DateTime.UtcNow;
        }

        return entry;
    }

    public static RateLimitPolicyStatus GetStatus(RateLimitPolicyEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Published is null)
        {
            return RateLimitPolicyStatus.Draft;
        }

        if (entry.Draft is null)
        {
            return RateLimitPolicyStatus.Published;
        }

        return AreEquivalent(entry.Draft, entry.Published)
            ? RateLimitPolicyStatus.Published
            : RateLimitPolicyStatus.PublishedWithDraft;
    }

    public static bool AreEquivalent(RateLimitPolicy left, RateLimitPolicy right)
    {
        if (left is null || right is null)
        {
            return left == right;
        }

        return JsonSerializer.Serialize(left, JOptions.Default) == JsonSerializer.Serialize(right, JOptions.Default);
    }
}

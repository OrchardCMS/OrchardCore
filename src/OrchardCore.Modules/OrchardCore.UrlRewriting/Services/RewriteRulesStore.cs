using OrchardCore.Documents;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class RewriteRulesStore
{
    private readonly IDocumentManager<RewriteRulesDocument> _documentManager;

    public RewriteRulesStore(IDocumentManager<RewriteRulesDocument> documentManager)
    {
        _documentManager = documentManager;
    }

    /// <summary>
    /// Loads the rewrite rules document from the store for updating and that should not be cached.
    /// </summary>
    public Task<RewriteRulesDocument> LoadRewriteRulesAsync()
        => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the rewrite rules document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<RewriteRulesDocument> GetRewriteRulesAsync()
        => _documentManager.GetOrCreateImmutableAsync();

    public async Task SaveAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();

        rules.Rules[rule.Id] = rule;

        await _documentManager.UpdateAsync(rules);
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();

        rules.Rules.Remove(rule.Id);

        await _documentManager.UpdateAsync(rules);
    }

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        var document = await GetRewriteRulesAsync();

        return document.Rules.TryGetValue(id, out var rule)
            ? rule
            : null;
    }
}

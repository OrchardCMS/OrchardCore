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
    public Task<RewriteRulesDocument> LoadRewriteRulesAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the rewrite rules document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<RewriteRulesDocument> GetRewriteRulesAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task SaveAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();

        var preexisting = rules.Rules.FirstOrDefault(x => string.Equals(x.Name, rule.Name, StringComparison.OrdinalIgnoreCase));

        // it's new? add it
        if (preexisting == null)
        {
            rules.Rules.Add(rule);
        }
        else // not new: replace it
        {
            var index = rules.Rules.IndexOf(preexisting);
            rules.Rules[index] = rule;
        }

        await _documentManager.UpdateAsync(rules);
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();
        var ruleToRemove = rules.Rules.FirstOrDefault(r => string.Equals(r.Name, rule.Name, StringComparison.OrdinalIgnoreCase));
        rules.Rules.Remove(ruleToRemove);

        await _documentManager.UpdateAsync(rules);
    }

    public async Task<RewriteRule> FindByIdAsync(string ruleId)
    {
        var rules = await GetRewriteRulesAsync();

        var rule = rules.Rules.FirstOrDefault(x => string.Equals(x.Name, ruleId, StringComparison.OrdinalIgnoreCase));

        if (rule == null)
        {
            return null;
        }

        return rule.Clone();
    }
}

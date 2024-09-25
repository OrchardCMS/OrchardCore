using OrchardCore.Documents;
using OrchardCore.Security;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public class RewriteRulesStore
{
    private readonly IDocumentManager<RewriteRulesDocument> _documentManager;

    private bool _updating;

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

    /// <summary>
    /// Updates the store with the provided rewrite rules document and then updates the cache.
    /// </summary>
    private Task UpdateRolesAsync(RewriteRulesDocument rules)
    {
        _updating = true;

        return _documentManager.UpdateAsync(rules);
    }

    public async Task CreateAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();
        rules.Rules.Add(rule);
        await UpdateRolesAsync(rules);
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        var rules = await LoadRewriteRulesAsync();
        var ruleToRemove = rules.Rules.FirstOrDefault(r => string.Equals(r.Name, rule.Name, StringComparison.OrdinalIgnoreCase));
        rules.Rules.Remove(ruleToRemove);

        await UpdateRolesAsync(rules);
    }

    public async Task<RewriteRule> FindByIdAsync(string ruleId)
    {
        // While updating find a rule from the loaded document being mutated.
        var rules = _updating ? await LoadRewriteRulesAsync() : await GetRewriteRulesAsync();

        var rule = rules.Rules.FirstOrDefault(x => string.Equals(x.Name, ruleId, StringComparison.OrdinalIgnoreCase));

        if (rule == null)
        {
            return null;
        }

        return _updating ? rule : rule.Clone();
    }
}

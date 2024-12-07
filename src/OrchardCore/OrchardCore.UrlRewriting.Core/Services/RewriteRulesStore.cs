using OrchardCore.Documents;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class RewriteRulesStore : IRewriteRulesStore
{
    private readonly IDocumentManager<RewriteRulesDocument> _documentManager;

    public RewriteRulesStore(IDocumentManager<RewriteRulesDocument> documentManager)
    {
        _documentManager = documentManager;
    }

    public async Task<IEnumerable<RewriteRule>> GetAllAsync()
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Rules.Values;
    }

    public async Task SaveAsync(RewriteRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var document = await _documentManager.GetOrCreateMutableAsync();

        document.Rules[rule.Id] = rule;

        await UpdateOrderAndSaveAsync(document.Rules.Values);
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        var document = await _documentManager.GetOrCreateMutableAsync();

        if (document.Rules.Remove(rule.Id))
        {
            await UpdateOrderAndSaveAsync(document.Rules.Values);
        }
    }

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Rules.TryGetValue(id, out var rule)
            ? rule
            : null;
    }

    public async Task<IEnumerable<RewriteRule>> UpdateOrderAndSaveAsync(IEnumerable<RewriteRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var order = 0;

        foreach (var rule in rules)
        {
            rule.Order = order++;
        }

        var document = await _documentManager.GetOrCreateMutableAsync();

        document.Rules = rules.ToDictionary(x => x.Id);

        await _documentManager.UpdateAsync(document);

        return document.Rules.Values;
    }
}

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
        var document = await _documentManager.GetOrCreateMutableAsync();

        document.Rules[rule.Id] = rule;

        await _documentManager.UpdateAsync(document);
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        var document = await _documentManager.GetOrCreateMutableAsync();

        document.Rules.Remove(rule.Id);

        await _documentManager.UpdateAsync(document);
    }

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        return document.Rules.TryGetValue(id, out var rule)
            ? rule
            : null;
    }
}

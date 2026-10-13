using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell.Scope;
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

        // Every change of the rules goes through this method, so this is where the rewrite options are refreshed.
        // The store is a singleton, so the scoped notifier is resolved from the current shell scope.
        ShellScope.Services?.GetService<IOptionsUpdateNotifier>()?.RequestUpdate<RewriteOptions>();

        return document.Rules.Values;
    }
}

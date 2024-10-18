using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public sealed class RewriteRulesManager : IRewriteRulesManager
{
    private readonly IRewriteRulesStore _store;
    private readonly IEnumerable<IRewriteRuleHandler> _rewriteRuleHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public RewriteRulesManager(
        IRewriteRulesStore store,
        IEnumerable<IRewriteRuleHandler> rewriteRuleHandlers,
        IServiceProvider serviceProvider,
        ILogger<RewriteRulesManager> logger)
    {
        _store = store;
        _rewriteRuleHandlers = rewriteRuleHandlers;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DeleteAsync(RewriteRule rule)
    {
        var deletingContext = new DeletingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, _logger);

        await _store.DeleteAsync(rule);

        var deletedContext = new DeletedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, _logger);
    }

    public async Task<RewriteValidateResult> ValidateAsync(RewriteRule rule)
    {
        var validatingContext = new ValidatingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        var validatedContext = new ValidatedRewriteRuleContext(rule, validatingContext.Result);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.ValidatedAsync(ctx), validatedContext, _logger);

        return validatingContext.Result;
    }

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        var rule = await _store.FindByIdAsync(id);

        if (rule != null)
        {
            await LoadAsync(rule);
        }

        return rule;
    }

    public async Task<RewriteRule> NewAsync(string source, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var ruleSource = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(source);

        if (ruleSource == null)
        {
            _logger.LogWarning("Unable to find a rule-source that can handle the source '{Source}'.", source);

            return null;
        }

        var id = IdGenerator.GenerateId();

        var rule = new RewriteRule()
        {
            Id = id,
            Source = source,
            Order = await GetNextOrderSequence()
        };

        var initializingContext = new InitializingRewriteRuleContext(rule, data);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        var initializedContext = new InitializedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        rule.Source = source;

        if (string.IsNullOrEmpty(rule.Id))
        {
            rule.Id = id;
        }

        return rule;
    }

    public async Task<IEnumerable<RewriteRule>> GetAllAsync()
    {
        var rules = await GetSortedRuleAsync();

        foreach (var rule in rules)
        {
            await LoadAsync(rule);
        }

        return rules;
    }

    public async Task UpdateAsync(RewriteRule rule, JsonNode data = null)
    {
        var updatingContext = new UpdatingRewriteRuleContext(rule, data);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatingContext, _logger);

        var updatedContext = new UpdatedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }

    public async Task SaveAsync(RewriteRule rule)
    {
        var savingContext = new SavingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavingAsync(ctx), savingContext, _logger);

        await _store.SaveAsync(rule);

        var savedContext = new SavedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavedAsync(ctx), savedContext, _logger);
    }

    public async Task ResortOrderAsync(int oldOrder, int newOrder)
    {
        if (oldOrder < 1 || newOrder < 1)
        {
            return;
        }

        var rules = (await GetSortedRuleAsync()).ToList();

        if (oldOrder > rules.Count || newOrder > rules.Count)
        {
            return;
        }

        var zeroBasedOldOrder = oldOrder - 1;
        var zeroBasedNewOrder = newOrder - 1;

        // Get the element to move.
        var ruleToMove = rules[zeroBasedOldOrder];

        // Remove the rule from its current position.
        rules.RemoveAt(zeroBasedOldOrder);

        rules.Insert(zeroBasedNewOrder, ruleToMove);

        await _store.UpdateOrderAndSaveAsync(rules);
    }

    private Task LoadAsync(RewriteRule rule)
    {
        var loadedContext = new LoadedRewriteRuleContext(rule);

        return _rewriteRuleHandlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }

    private async Task<IEnumerable<RewriteRule>> GetSortedRuleAsync()
    {
        var rules = await _store.GetAllAsync();

        return rules.OrderBy(x => x.Order)
            .ThenBy(x => x.CreatedUtc);
    }

    private async Task<int> GetNextOrderSequence()
    {
        var rules = await _store.GetAllAsync();

        // When importing multiple rules using a recipe, the rules collection will not include the newly added rule.
        // To address this, we maintain an internal counter managed by this scoped service.
        return rules.Any()
            ? rules.Max(x => x.Order) + ++_counter
            : _counter++;
    }

    private int _counter;
}

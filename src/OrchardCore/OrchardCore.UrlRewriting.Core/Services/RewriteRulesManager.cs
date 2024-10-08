using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

public class RewriteRulesManager : IRewriteRulesManager
{
    private readonly IRewriteRulesStore _store;
    private readonly IEnumerable<IRewriteRuleHandler> _rewriteRuleHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RewriteRulesManager> _logger;

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

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        var rule = await _store.FindByIdAsync(id);

        if (rule == null)
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

        var order = await GenerateNextAvaliableOrderNumber();

        var rule = new RewriteRule()
        {
            Id = IdGenerator.GenerateId(),
            Source = source,
            Order = order
        };

        var initializingContext = new InitializingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        if (data != null)
        {
            rule.Name = data[nameof(RewriteRule.Name)]?.GetValue<string>();
        }

        var initializedContext = new InitializedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        rule.Source = source;
        rule.Id ??= IdGenerator.GenerateId();
        rule.Order = order;

        return rule;
    }

    public async Task<ListRewriteRuleResult> PageRulesAsync(int page, int pageSize, RewriteRulesQueryContext context = null)
    {
        var records = await LocateRulesAsync(context);

        var skip = (page - 1) * pageSize;

        var result = new ListRewriteRuleResult
        {
            Count = records.Count(),
            Records = records.Skip(skip).Take(pageSize).ToArray()
        };

        foreach (var record in result.Records)
        {
            await LoadAsync(record);
        }

        return result;
    }

    public async Task SaveAsync(RewriteRule rule)
    {
        var savingContext = new SavingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavingAsync(ctx), savingContext, _logger);

        await _store.SaveAsync(rule);

        var savedContext = new SavedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavedAsync(ctx), savedContext, _logger);
    }

    private Task LoadAsync(RewriteRule rule)
    {
        var loadedContext = new LoadedRewriteRuleContext(rule);

        return _rewriteRuleHandlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }

    private async Task<IEnumerable<RewriteRule>> LocateRulesAsync(RewriteRulesQueryContext context)
    {
        var rules = await _store.GetAllAsync();

        if (context == null)
        {
            return rules;
        }

        if (!string.IsNullOrEmpty(context.Source))
        {
            rules = rules.Where(x => x.Source.Equals(context.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(context.Name))
        {
            rules = rules.Where(x => x.Name.Contains(context.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (context.Sorted)
        {
            rules = rules.OrderBy(x => x.Order)
                .ThenBy(x => x.CreatedUtc);
        }

        return rules;
    }

    private async Task<int> GenerateNextAvaliableOrderNumber()
    {
        var rules = await LocateRulesAsync(new RewriteRulesQueryContext() { Sorted = true });

        return rules.LastOrDefault()?.Order + 1 ?? 0;
    }
}

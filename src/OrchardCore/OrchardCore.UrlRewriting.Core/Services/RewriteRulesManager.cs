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

    public Task DeleteAsync(RewriteRule rule)
        => _store.DeleteAsync(rule);

    public async Task<RewriteRule> FindByIdAsync(string id)
    {
        var rule = await _store.FindByIdAsync(id);

        if (rule == null)
        {
            await LoadAsync(rule);
        }

        return rule;
    }

    public async Task<RewriteRule> NewAsync(string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var ruleSource = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(source);

        if (ruleSource == null)
        {
            _logger.LogWarning("Unable to find a rule-source that can handle the source '{Source}'.", source);

            return null;
        }

        var rule = new RewriteRule()
        {
            Id = IdGenerator.GenerateId(),
            Source = source,
        };

        var initializingContext = new InitializingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        var initializedContext = new InitializedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        rule.Source = source;

        return rule;
    }

    public async Task<ListRewriteRuleResult> PageQueriesAsync(int page, int pageSize, RewriteRulesQueryContext context = null)
    {
        var records = await LocateQueriesAsync(context);

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

    public Task SaveAsync(RewriteRule rule)
    {
        throw new NotImplementedException();
    }

    private Task LoadAsync(RewriteRule rule)
    {
        var loadedContext = new LoadedRewriteRuleContext(rule);

        return _rewriteRuleHandlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }

    private async Task<IEnumerable<RewriteRule>> LocateQueriesAsync(RewriteRulesQueryContext context)
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
            rules = rules.OrderBy(x => x.Name);
        }

        return rules;
    }
}

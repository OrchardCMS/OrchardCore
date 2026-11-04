using System.Text.Json.Nodes;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Environment.Shell;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Services;

/// <summary>Manages stored rewrite rules, lifecycle handlers, validation and tenant reloads.</summary>
public sealed class RewriteRulesManager : IRewriteRulesManager
{
    private readonly IRewriteRulesStore _store;
    private readonly IEnumerable<IRewriteRuleHandler> _rewriteRuleHandlers;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IShellReleaseManager _releaseManager;
    internal readonly IStringLocalizer S;

    /// <summary>Creates the tenant rewrite-rule manager.</summary>
    public RewriteRulesManager(
        IRewriteRulesStore store,
        IEnumerable<IRewriteRuleHandler> rewriteRuleHandlers,
        IServiceProvider serviceProvider,
        ILogger<RewriteRulesManager> logger,
        IStringLocalizer<RewriteRulesManager> localizer,
        IShellReleaseManager releaseManager)
    {
        _store = store;
        _rewriteRuleHandlers = rewriteRuleHandlers;
        _serviceProvider = serviceProvider;
        _logger = logger;
        S = localizer;
        _releaseManager = releaseManager;
    }

    /// <summary>Deletes an existing rule and requests a reload; a missing rule is a no-op.</summary>
    public async Task DeleteAsync(RewriteRule rule)
    {
        if (await _store.FindByIdAsync(rule.Id) is null)
        {
            return;
        }
        var deletingContext = new DeletingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, _logger);

        await _store.DeleteAsync(rule);
        _releaseManager.RequestRelease();

        var deletedContext = new DeletedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, _logger);
    }

    /// <summary>Validates rule handlers and verifies the source can construct its runtime rule.</summary>
    public async Task<RewriteValidateResult> ValidateAsync(RewriteRule rule)
    {
        var validatingContext = new ValidatingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        if (validatingContext.Result.Succeeded)
        {
            var source = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(rule.Source);
            if (source is null)
            {
                validatingContext.Result.Fail(new ValidationResult(S["The rule source is not available."], [nameof(rule.Source)]));
            }
            else
            {
                try
                {
                    source.Configure(new RewriteOptions(), rule);
                }
                catch (Exception exception) when (exception is ArgumentException or FormatException)
                {
                    validatingContext.Result.Fail(new ValidationResult(S["The rule cannot be parsed by its runtime source."], [nameof(rule.Properties)]));
                }
            }
        }

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
            Order = await GetNextOrderSequence(),
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

    /// <summary>Saves changed rule data and requests a reload, preserving unchanged retries.</summary>
    public async Task SaveAsync(RewriteRule rule)
    {
        var current = await _store.FindByIdAsync(rule.Id);
        if (current is not null && current.Name == rule.Name && current.Source == rule.Source
            && current.Order == rule.Order && current.CreatedUtc == rule.CreatedUtc
            && current.OwnerId == rule.OwnerId && current.Author == rule.Author
            && JsonNode.DeepEquals(current.Properties, rule.Properties))
        {
            return;
        }
        var savingContext = new SavingRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavingAsync(ctx), savingContext, _logger);

        await _store.SaveAsync(rule);
        _releaseManager.RequestRelease();

        var savedContext = new SavedRewriteRuleContext(rule);
        await _rewriteRuleHandlers.InvokeAsync((handler, ctx) => handler.SavedAsync(ctx), savedContext, _logger);
    }

    /// <summary>Moves a rule between one-based positions, matching the admin list with its header row.</summary>
    public async Task ResortOrderAsync(int oldOrder, int newOrder)
    {
        if (oldOrder < 1 || newOrder < 1 || oldOrder == newOrder)
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
        _releaseManager.RequestRelease();
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

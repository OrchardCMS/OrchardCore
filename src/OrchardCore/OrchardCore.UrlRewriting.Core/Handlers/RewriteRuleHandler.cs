using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Handlers;

public sealed class RewriteRuleHandler : RewriteRuleHandlerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;

    internal readonly IStringLocalizer S;

    public RewriteRuleHandler(
        IHttpContextAccessor httpContextAccessor,
        IClock clock,
        IStringLocalizer<RewriteRuleHandler> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
        S = stringLocalizer;
    }

    public override Task InitializingAsync(InitializingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task UpdatingAsync(UpdatingRewriteRuleContext context)
        => PopulateAsync(context.Rule, context.Data);

    public override Task ValidatingAsync(ValidatingRewriteRuleContext context)
    {
        if (string.IsNullOrWhiteSpace(context.Rule.Name))
        {
            context.Result.Fail(new ValidationResult(S["Rule name is required"], [nameof(RewriteRule.Name)]));
        }

        if (string.IsNullOrWhiteSpace(context.Rule.Source))
        {
            context.Result.Fail(new ValidationResult(S["Source name is required"], [nameof(RewriteRule.Source)]));
        }

        return Task.CompletedTask;
    }

    public override Task InitializedAsync(InitializedRewriteRuleContext context)
    {
        context.Rule.CreatedUtc = _clock.UtcNow;

        var user = _httpContextAccessor.HttpContext?.User;

        if (user != null)
        {
            context.Rule.OwnerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            context.Rule.Author = user.Identity.Name;
        }

        return Task.CompletedTask;
    }

    private static Task PopulateAsync(RewriteRule rule, JsonNode data)
    {
        var name = data[nameof(RewriteRule.Name)]?.GetValue<string>();

        if (!string.IsNullOrEmpty(name))
        {
            rule.Name = name;
        }

        return Task.CompletedTask;
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Handler;

public class UpdateRewriteRuleHandler : RewriteRuleHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;

    public UpdateRewriteRuleHandler(
        IHttpContextAccessor httpContextAccessor,
        IClock clock)
    {
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
    }

    public override Task InitializedAsync(InitializedRewriteRuleContext context)
    {
        context.Rule.OwnerId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        context.Rule.Author = _httpContextAccessor.HttpContext.User.Identity.Name;
        context.Rule.CreatedUtc = _clock.UtcNow;

        return Task.CompletedTask;
    }
}

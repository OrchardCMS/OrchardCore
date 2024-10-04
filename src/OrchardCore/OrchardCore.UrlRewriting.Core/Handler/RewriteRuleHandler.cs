using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.Handler;

public abstract class RewriteRuleHandler : IRewriteRuleHandler
{
    public virtual Task InitializedAsync(InitializedRewriteRuleContext context)
        => Task.CompletedTask;

    public virtual Task InitializingAsync(InitializingRewriteRuleContext context)
        => Task.CompletedTask;

    public virtual Task LoadedAsync(LoadedRewriteRuleContext context)
        => Task.CompletedTask;
}

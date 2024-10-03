using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IRewriteRulesManager
{
    Task DeleteAsync(RewriteRule rule);

    Task<RewriteRule> FindByIdAsync(string id);

    Task<RewriteRule> NewAsync(string source);

    Task<ListRewriteRuleResult> PageQueriesAsync(int page, int pageSize, RewriteRulesQueryContext context = null);

    Task SaveAsync(RewriteRule rule);
}

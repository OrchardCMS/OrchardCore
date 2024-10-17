using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IRewriteRulesStore
{
    Task DeleteAsync(RewriteRule rule);

    Task<RewriteRule> FindByIdAsync(string id);

    Task<IEnumerable<RewriteRule>> GetAllAsync();

    Task SaveAsync(RewriteRule rule);

    Task<IEnumerable<RewriteRule>> UpdateOrderAndSaveAsync(IEnumerable<RewriteRule> rules);
}

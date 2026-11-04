using System.Text.Json.Nodes;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IRewriteRulesManager
{
    Task<RewriteRule> NewAsync(string source, JsonNode data = null);

    /// <summary>Validates the rule using its registered handlers and runtime source.</summary>
    Task<RewriteValidateResult> ValidateAsync(RewriteRule rule);

    Task<RewriteRule> FindByIdAsync(string id);

    /// <summary>Saves a changed rule and requests a tenant reload. Validate before saving.</summary>
    Task SaveAsync(RewriteRule rule);

    /// <summary>Deletes an existing rule and requests a tenant reload.</summary>
    Task DeleteAsync(RewriteRule rule);

    Task UpdateAsync(RewriteRule rule, JsonNode data = null);

    /// <summary>Moves a rule between one-based positions and reloads the tenant when changed.</summary>
    Task ResortOrderAsync(int oldOrder, int newOrder);

    Task<IEnumerable<RewriteRule>> GetAllAsync();
}

using System.Text.Json.Nodes;

namespace OrchardCore.UrlRewriting.Models;

public sealed class UpdatingRewriteRuleContext : RewriteRuleContextBase
{
    public JsonNode Data { get; }

    public UpdatingRewriteRuleContext(RewriteRule rule, JsonNode data)
        : base(rule)
    {
        Data = data ?? new JsonObject();
    }
}

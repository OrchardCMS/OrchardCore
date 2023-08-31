using OrchardCore.Secrets.Models;

namespace OrchardCore.Workflows.Http.Models;

public class HttpRequestEventSecret : SecretBase
{
    public string WorkflowTypeId { get; set; }
    public string ActivityId { get; set; }
    public int TokenLifeSpan { get; set; }
}

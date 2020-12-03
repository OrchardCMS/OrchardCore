using OrchardCore.Workflows.Activities;
using OrchardCore.Secrets;

namespace OrchardCore.Workflows.Models
{
    public class HttpRequestEventSecret : Secret
    {
        public string WorkflowTypeId { get; set; }
        public string ActivityId { get; set; }
        public int TokenLifeSpan { get; set; }
        public string Token { get; set; }
        // TODO possibly more in here for method / antiforgery etc?
    }
}

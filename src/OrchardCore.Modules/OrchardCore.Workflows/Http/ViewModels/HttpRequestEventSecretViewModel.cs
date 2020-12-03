
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRequestEventSecretViewModel
    {
        public string WorkflowTypeId { get; set; }
        public string ActivityId { get; set; }
        public int TokenLifeSpan { get; set; }
        public string Token { get; set; }
        
        [BindNever]
        public BuildEditorContext Context { get; set; }
    }
}

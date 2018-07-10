using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Forms.Models;
using OrchardCore.Forms.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Handlers
{
    public class FormWorkflowPartHandler : ContentPartHandler<FormPart>
    {
        public override Task ActivatingAsync(ActivatingContentContext context, FormPart instance)
        {
            context.Builder.Weld(nameof(FormWorkflowPart), new FormWorkflowPart());
            return Task.CompletedTask;
        }
    }
}
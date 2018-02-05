using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    /// <summary>
    /// Implementors serialize complex external stimuli into a serializable format.
    /// for example, when triggering a workflow and passing in a content item, only the content item's ID is persisted and re-hydrated
    /// when the workflow is resumed.
    /// </summary>
    public interface IWorkflowValueSerializer
    {
        Task SerializeValueAsync(SerializeWorkflowValueContext context);
        Task DeserializeValueAsync(SerializeWorkflowValueContext context);
    }
}

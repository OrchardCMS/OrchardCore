using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowValueSerializer
    {
        Task SerializeValueAsync(SerializeWorkflowValueContext context);
        Task DeserializeValueAsync(SerializeWorkflowValueContext context);
    }
}

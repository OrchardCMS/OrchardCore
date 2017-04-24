using System.Threading.Tasks;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Deployment.Editors
{
    public interface IDeploymentStepDisplayManager
    {
        Task<dynamic> DisplayStepAsync(DeploymentStep step, IUpdateModel updater, string displayType);
        Task<dynamic> BuildStepEditorAsync(DeploymentStep step, IUpdateModel updater);
        Task<dynamic> UpdatStepEditorAsync(DeploymentStep step, IUpdateModel updater);
    }
}

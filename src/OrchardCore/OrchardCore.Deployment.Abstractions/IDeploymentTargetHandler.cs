using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Deployment
{
    public interface IDeploymentTargetHandler
    {
        Task ImportFromFileAsync(IFileProvider fileProvider);
    }
}

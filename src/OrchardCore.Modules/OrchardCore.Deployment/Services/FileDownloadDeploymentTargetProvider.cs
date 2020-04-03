using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class FileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
    {
        public FileDownloadDeploymentTargetProvider(IStringLocalizer<FileDownloadDeploymentTargetProvider> stringLocalizer)
        {
            T = stringLocalizer;
        }

        public IStringLocalizer T { get; }

        public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            return Task.FromResult<IEnumerable<DeploymentTarget>>(
                new [] {
                    new DeploymentTarget(
                        name: T["File Download"],
                        description: T["Download a deployment plan locally."],
                        route: new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Deployment",
                            controller = "ExportFile",
                            action = "Execute"
                        })
                    )
                }
            );
        }
    }
}

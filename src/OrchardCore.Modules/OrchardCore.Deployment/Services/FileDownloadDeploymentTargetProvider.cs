using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class FileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S;
#pragma warning restore IDE1006 // Naming Styles

        public FileDownloadDeploymentTargetProvider(IStringLocalizer<FileDownloadDeploymentTargetProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            return Task.FromResult<IEnumerable<DeploymentTarget>>(
                new[] {
                    new DeploymentTarget(
                        name: S["File Download"],
                        description: S["Download a deployment plan locally."],
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

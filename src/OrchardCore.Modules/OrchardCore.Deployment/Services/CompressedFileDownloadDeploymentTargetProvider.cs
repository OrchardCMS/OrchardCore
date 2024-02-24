using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class CompressedFileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
    {
        protected readonly IStringLocalizer S;

        public CompressedFileDownloadDeploymentTargetProvider(IStringLocalizer<CompressedFileDownloadDeploymentTargetProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            return Task.FromResult<IEnumerable<DeploymentTarget>>(
                new[] {
                    new DeploymentTarget(
                        name: S["Compressed File Download"],
                        description: S["Download a compressed deployment plan locally."],
                        route: new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Deployment",
                            controller = "ExportFile",
                            action = "ExecuteCompressed"
                        })
                    )
                }
            );
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class UncompressedFileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
    {
        protected readonly IStringLocalizer S;

        public UncompressedFileDownloadDeploymentTargetProvider(IStringLocalizer<CompressedFileDownloadDeploymentTargetProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            return Task.FromResult<IEnumerable<DeploymentTarget>>(
                new[] {
                    new DeploymentTarget(
                        name: S["Uncompressed File Download"],
                        description: S["Download an uncompressed deployment plan locally."],
                        route: new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Deployment",
                            controller = "ExportFile",
                            action = "ExecuteUncompressed"
                        })
                    )
                }
            );
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment
{
    public class FileDownloadDeploymentTargetProvider : IDeploymentTargetProvider
    {
        protected readonly IStringLocalizer S;

        public FileDownloadDeploymentTargetProvider(IStringLocalizer<FileDownloadDeploymentTargetProvider> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            var target = new DeploymentTarget(
                        name: S["File Download"],
                        description: S["Download a deployment plan locally."],
                        route: new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Deployment",
                            controller = "ExportFile",
                            action = "Execute"
                        })
                    )
            {
                Formats =
                [
                    new SelectListItem(S["Compressed"], "zip"),
                    new SelectListItem(S["Uncompressed"], "json"),
                ]
            };

            return Task.FromResult<IEnumerable<DeploymentTarget>>([target]);
        }
    }
}

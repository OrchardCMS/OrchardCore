using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.Remote.Services;

namespace OrchardCore.Deployment
{
    public class RemoteInstanceDeploymentTargetProvider : IDeploymentTargetProvider
    {
        private readonly RemoteInstanceService _service;
        protected readonly IStringLocalizer S;

        public RemoteInstanceDeploymentTargetProvider(
            IStringLocalizer<RemoteInstanceDeploymentTargetProvider> stringLocalizer,
            RemoteInstanceService service)
        {
            _service = service;
            S = stringLocalizer;
        }

        public async Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            var remoteInstanceList = await _service.GetRemoteInstanceListAsync();

            return remoteInstanceList.RemoteInstances.Select(x =>
                    new DeploymentTarget(
                        name: new LocalizedString(x.Name, x.Name, false),
                        description: S["Sends the deployment plan to a remote instance."],
                        route: new RouteValueDictionary(new
                        {
                            area = "OrchardCore.Deployment.Remote",
                            controller = "ExportRemoteInstance",
                            action = "Execute",
                            remoteInstanceId = x.Id
                        })
                    )
                ).ToArray();
        }
    }
}

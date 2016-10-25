using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Orchard.Deployment.Remote.Services;

namespace Orchard.Deployment
{
    public class RemoteInstanceDeploymentTargetProvider : IDeploymentTargetProvider
    {
        private readonly RemoteInstanceService _service;

        public RemoteInstanceDeploymentTargetProvider(
            IStringLocalizer<RemoteInstanceDeploymentTargetProvider> stringLocalizer,
            RemoteInstanceService service)
        {
            _service = service;
            T = stringLocalizer;
        }

        public IStringLocalizer T { get; }

        public async Task<IEnumerable<DeploymentTarget>> GetDeploymentTargetsAsync()
        {
            var remoteInstanceList = await _service.GetRemoteInstanceListAsync(); ;

            return remoteInstanceList.RemoteInstances.Select(x =>
                    new DeploymentTarget(
                        name: new LocalizedString(x.Name, x.Name, false),
                        description: T["Sends the deployment plan to a remote instance."],
                        route: new RouteValueDictionary(new
                        {
                            area = "Orchard.Deployment.Remote",
                            controller = "ExportRemoteInstance",
                            action = "Execute",
                            remoteInstanceId = x.Id
                        })
                    )
                ).ToArray();
        }
    }
}

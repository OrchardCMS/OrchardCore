using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentSource : IDeploymentSource
    {
        private readonly IAdminMenuService _adminMenuService;

        public AdminMenuDeploymentSource(IAdminMenuService adminMenuService)
        {
            _adminMenuService = adminMenuService;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep step, DeploymentPlanResult result)
        {
            var adminMenuStep = step as AdminMenuDeploymentStep;

            if (adminMenuStep == null)
            {
                return;
            }

            var data = new JsonArray();
            result.AddSimpleStep("AdminMenu", "data", data);

            // For each AdminNode, store info about its concrete type: linkAdminNode, contentTypesAdminNode etc...
            var serializer = new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
            {
                var objectData = JsonSerializer.SerializeToNode(adminMenu, serializer);
                data.Add<AdminMenu>(adminMenu);
            }

            return;
        }
    }
}

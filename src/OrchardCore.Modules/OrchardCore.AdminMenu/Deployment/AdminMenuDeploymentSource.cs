using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Deployment;
using OrchardCore.Json;

namespace OrchardCore.AdminMenu.Deployment;

public class AdminMenuDeploymentSource
    : DeploymentSourceBase<AdminMenuDeploymentStep>
{
    private readonly IAdminMenuService _adminMenuService;
    private readonly JsonSerializerOptions _serializationOptions;

    public AdminMenuDeploymentSource(IAdminMenuService adminMenuService,
        IOptions<DocumentJsonSerializerOptions> serializationOptions)
    {
        _adminMenuService = adminMenuService;
        _serializationOptions = serializationOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(AdminMenuDeploymentStep step, DeploymentPlanResult result)
    {
        var data = new JsonArray();
        result.Steps.Add(new JsonObject
        {
            ["name"] = "AdminMenu",
            ["data"] = data,
        });

        foreach (var adminMenu in (await _adminMenuService.GetAdminMenuListAsync()).AdminMenu)
        {
            var objectData = JObject.FromObject(adminMenu, _serializationOptions);
            data.Add(objectData);
        }
    }
}

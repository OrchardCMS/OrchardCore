using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.Deployment.Services;
using OrchardCore.Mvc.Utilities;
using YesSql;

namespace OrchardCore.Deployment.Controllers;

[Admin("DeploymentPlan/ExportFile/{action}/{id?}", "DeploymentPlanExportFile{action}")]
public sealed class ExportFileController : Controller
{
    private readonly IDeploymentArchiveService _archives;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;

    /// <summary>Creates the admin download action with shared archive generation.</summary>
    public ExportFileController(
        IAuthorizationService authorizationService,
        ISession session,
        IDeploymentArchiveService archives)
    {
        _authorizationService = authorizationService;
        _archives = archives;
        _session = session;
    }

    [HttpPost]
    public async Task<IActionResult> Execute(long id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Export))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var filename = deploymentPlan.Name.ToSafeName() + ".zip";

        var recipeDescriptor = DeploymentRecipeMetadata.Create(deploymentPlan);

        return new FileStreamResult(await _archives.CreateAsync(deploymentPlan, recipeDescriptor), MediaTypeNames.Application.Zip) { FileDownloadName = filename };
    }
}

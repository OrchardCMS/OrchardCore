using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Recipes.Models;
using YesSql;

namespace OrchardCore.Deployment.Remote.Controllers;

[Admin("Deployment/ExportRemoteInstance/{action}/{id?}", "DeploymentExportRemoteInstance{action}")]
public sealed class ExportRemoteInstanceController : Controller
{
    private readonly IDeploymentArchiveService _archives;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly RemoteInstanceService _service;
    private readonly INotifier _notifier;
    private readonly IHttpClientFactory _httpClientFactory;

    internal readonly IHtmlLocalizer H;

    /// <summary>Creates the existing remote export action with shared archive generation.</summary>
    public ExportRemoteInstanceController(
        IAuthorizationService authorizationService,
        ISession session,
        RemoteInstanceService service,
        IDeploymentArchiveService archives,
        INotifier notifier,
        IHttpClientFactory httpClientFactory,
        IHtmlLocalizer<ExportRemoteInstanceController> localizer)
    {
        _authorizationService = authorizationService;
        _archives = archives;
        _session = session;
        _service = service;
        _notifier = notifier;
        _httpClientFactory = httpClientFactory;
        H = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> Execute(long id, string remoteInstanceId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.Export)
            || !await _authorizationService.AuthorizeAsync(User, DeploymentPermissions.ExportRemoteInstances))
        {
            return Forbid();
        }

        var deploymentPlan = await _session.GetAsync<DeploymentPlan>(id);

        if (deploymentPlan == null)
        {
            return NotFound();
        }

        var remoteInstance = await _service.GetRemoteInstanceAsync(remoteInstanceId);

        if (remoteInstance == null)
        {
            return NotFound();
        }

        var filename = deploymentPlan.Name.ToSafeName() + ".zip";
        await using var archive = await _archives.CreateAsync(deploymentPlan, new RecipeDescriptor());

        var status = await new RemoteDeploymentSender(_httpClientFactory).SendAsync(remoteInstance, archive, filename, HttpContext.RequestAborted);

        if (status == System.Net.HttpStatusCode.OK)
        {
            await _notifier.SuccessAsync(H["Deployment executed successfully."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while sending the deployment to the remote instance: \"{0} ({1})\"", status.ToString(), (int)status]);
        }

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return this.LocalRedirect(returnUrl, true);
        }

        return RedirectToAction("Display", "DeploymentPlan", new { area = "OrchardCore.Deployment", id });
    }
}

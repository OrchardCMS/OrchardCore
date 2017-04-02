using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
using Orchard.Admin;
using Orchard.Deployment.Core.Services;
using Orchard.Deployment.Remote.Models;
using Orchard.Deployment.Remote.Services;
using Orchard.Deployment.Services;
using Orchard.DisplayManagement.Notify;
using Microsoft.AspNetCore.Mvc.Modules.Utilities;
using YesSql.Core.Services;

namespace Orchard.Deployment.Remote.Controllers
{
    [Admin]
    public class ExportRemoteInstanceController : Controller
    {
        private readonly IDeploymentManager _deploymentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly RemoteInstanceService _service;
        private readonly INotifier _notifier;

        public ExportRemoteInstanceController(
            IAuthorizationService authorizationService,
            ISession session,
            RemoteInstanceService service,
            IDeploymentManager deploymentManager,
            INotifier notifier,
            IHtmlLocalizer<ExportRemoteInstanceController> h)
        {
            _authorizationService = authorizationService;
            _deploymentManager = deploymentManager;
            _session = session;
            _service = service;
            _notifier = notifier;
            H = h;
        }

        public IHtmlLocalizer H { get; }

        [HttpPost]
        public async Task<IActionResult> Execute(int id, string remoteInstanceId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.Export))
            {
                return Unauthorized();
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

            string archiveFileName;
            var filename = deploymentPlan.Name.ToSafeName() + ".zip";

            using (var fileBuilder = new TemporaryFileBuilder())
            {
                archiveFileName = Path.Combine(Path.GetTempPath(), filename);

                var deploymentPlanResult = new DeploymentPlanResult(fileBuilder);
                await _deploymentManager.ExecuteDeploymentPlanAsync(deploymentPlan, deploymentPlanResult);

                if (System.IO.File.Exists(archiveFileName))
                {
                    System.IO.File.Delete(archiveFileName);
                }

                ZipFile.CreateFromDirectory(fileBuilder.Folder, archiveFileName);
            }

            HttpResponseMessage response;

            try
            {
                var archiveContainer = new ArchiveContainer();
                archiveContainer.ClientName = remoteInstance.ClientName;
                archiveContainer.ApiKey = remoteInstance.ApiKey;
                archiveContainer.ArchiveBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(archiveFileName));
                
                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(archiveContainer));
                    content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                    response = await httpClient.PostAsync(remoteInstance.Url, content);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _notifier.Success(H["Deployment executed successfully."]);
                }
                else
                {
                    _notifier.Error(H["An error occured while sending the deployment to the remote instance: \"{0} ({1})\"", response.ReasonPhrase, (int)response.StatusCode]);
                }
            }
            finally
            {
                System.IO.File.Delete(archiveFileName);
            }
            
            return RedirectToAction("Display", "DeploymentPlan", new { area = "Orchard.Deployment", id });
        }
    }
}

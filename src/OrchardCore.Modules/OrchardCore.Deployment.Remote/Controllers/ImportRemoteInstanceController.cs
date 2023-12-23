using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Deployment.Remote.Controllers
{
    public class ImportRemoteInstanceController : Controller
    {
        private readonly RemoteClientService _remoteClientService;
        private readonly IDeploymentManager _deploymentManager;
        private readonly ISecretService _secretService;

        public ImportRemoteInstanceController(
            RemoteClientService remoteClientService,
            IDeploymentManager deploymentManager,
            ISecretService secretService)
        {
            _remoteClientService = remoteClientService;
            _deploymentManager = deploymentManager;
            _secretService = secretService;
        }

        /// <remarks>
        /// We ignore the AFT as the service is called from external applications (they can't have valid ones) and
        /// we use a private API key to secure its calls.
        /// </remarks>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Import(ImportViewModel model)
        {
            var remoteClientList = await _remoteClientService.GetRemoteClientListAsync();

            var remoteClient = remoteClientList.RemoteClients.FirstOrDefault(remote => remote.ClientName == model.ClientName);
            if (remoteClient is null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "The remote client was not provided.");
            }

            var secret = await _secretService.GetSecretAsync<TextSecret>($"{RemoteSecret.Namespace}.{model.ClientName}.ApiKey");
            if (secret is null || secret.Text != model.ApiKey)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "The Api Key was not recognized.");
            }

            // Create a temporary filename to save the archive
            var tempArchiveName = Path.GetTempFileName() + ".zip";

            // Create a temporary folder to extract the archive to
            var tempArchiveFolder = PathExtensions.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                using (var fs = System.IO.File.Create(tempArchiveName))
                {
                    await model.Content.CopyToAsync(fs);
                }

                ZipFile.ExtractToDirectory(tempArchiveName, tempArchiveFolder);

                await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));
            }
            finally
            {
                if (System.IO.File.Exists(tempArchiveName))
                {
                    System.IO.File.Delete(tempArchiveName);
                }

                if (Directory.Exists(tempArchiveFolder))
                {
                    Directory.Delete(tempArchiveFolder, true);
                }
            }

            return Ok();
        }
    }
}

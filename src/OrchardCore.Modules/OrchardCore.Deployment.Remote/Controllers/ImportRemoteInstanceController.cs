using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Remote.Controllers
{
    public class ImportRemoteInstanceController : Controller
    {
        private readonly RemoteClientService _remoteClientService;
        private readonly IDeploymentManager _deploymentManager;
        private readonly IDataProtector _dataProtector;

        public ImportRemoteInstanceController(
            IDataProtectionProvider dataProtectionProvider,
            RemoteClientService remoteClientService,
            IDeploymentManager deploymentManager)
        {
            _deploymentManager = deploymentManager;
            _remoteClientService = remoteClientService;
            _dataProtector = dataProtectionProvider.CreateProtector("OrchardCore.Deployment").ToTimeLimitedDataProtector();
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

            var remoteClient = remoteClientList.RemoteClients.FirstOrDefault(x => x.ClientName == model.ClientName);

            if (remoteClient == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "The remote client was not provided");
            }

            var apiKey = Encoding.UTF8.GetString(_dataProtector.Unprotect(remoteClient.ProtectedApiKey));

            if (model.ApiKey != apiKey || model.ClientName != remoteClient.ClientName)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "The Api Key was not recognized");
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

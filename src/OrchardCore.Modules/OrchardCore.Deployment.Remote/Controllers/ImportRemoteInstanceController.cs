using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Services;
using YesSql;

namespace OrchardCore.Deployment.Remote.Controllers
{
    public class ImportRemoteInstanceController : Controller
    {
        private readonly IDeploymentManager _deploymentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;
        private readonly RemoteClientService _remoteClientService;
        private readonly IEnumerable<IDeploymentTargetHandler> _deploymentTargetHandlers;

        public ImportRemoteInstanceController(
            IAuthorizationService authorizationService,
            ISession session,
            RemoteClientService remoteClientService,
            IDeploymentManager deploymentManager,
            IEnumerable<IDeploymentTargetHandler> deploymentTargetHandlers,
            IHtmlLocalizer<ExportRemoteInstanceController> h)
        {
            _authorizationService = authorizationService;
            _deploymentManager = deploymentManager;
            _session = session;
            _remoteClientService = remoteClientService;
            _deploymentTargetHandlers = deploymentTargetHandlers;
            H = h;
        }

        public IHtmlLocalizer H { get; }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Import([FromBody] ArchiveContainer archive)
        {
            var remoteClientList = await _remoteClientService.GetRemoteClientListAsync();

            var remoteClient = remoteClientList.RemoteClients.FirstOrDefault(x => x.ClientName == archive.ClientName);

            if (remoteClient == null || archive.ApiKey != remoteClient.ApiKey || archive.ClientName != remoteClient.ClientName)
            {
                return StatusCode(403, "The Api Key was not recognized");
            }

            var tempArchiveName = Path.GetTempFileName() + ".zip";
            var tempArchiveFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            try
            {
                byte[] content = Convert.FromBase64String(archive.ArchiveBase64);
                System.IO.File.WriteAllBytes(tempArchiveName, content);

                ZipFile.ExtractToDirectory(tempArchiveName, tempArchiveFolder);

                var fileProvider = new PhysicalFileProvider(tempArchiveFolder);

                foreach(var deploymentTargetHandler in _deploymentTargetHandlers)
                {
                    // Don't trigger in parallel to avoid potential race conditions in the handlers
                    await deploymentTargetHandler.ImportFromFileAsync(fileProvider);
                }
            }
            finally
            {
                if(System.IO.File.Exists(tempArchiveName))
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

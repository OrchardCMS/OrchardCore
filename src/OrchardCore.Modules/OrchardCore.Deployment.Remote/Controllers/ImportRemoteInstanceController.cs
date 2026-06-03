using System.IO.Compression;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.Deployment.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment.Remote.Controllers;

public sealed class ImportRemoteInstanceController : Controller
{
    private readonly RemoteClientService _remoteClientService;
    private readonly IDeploymentManager _deploymentManager;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly IDataProtector _dataProtector;
    private readonly IAntivirusScanner _fileUploadScanner;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public ImportRemoteInstanceController(
        IDataProtectionProvider dataProtectionProvider,
        RemoteClientService remoteClientService,
        IDeploymentManager deploymentManager,
        IAntivirusScanner fileUploadScanner,
        INotifier notifier,
        IHtmlLocalizer<ImportRemoteInstanceController> htmlLocalizer,
        IStringLocalizer<ImportRemoteInstanceController> stringLocalizer,
        ILogger<ImportRemoteInstanceController> logger)
    {
        _deploymentManager = deploymentManager;
        _fileUploadScanner = fileUploadScanner;
        _notifier = notifier;
        _logger = logger;
        _remoteClientService = remoteClientService;
        H = htmlLocalizer;
        S = stringLocalizer;
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
        var tempArchiveName = PathExtensions.GetTempFileName() + ".zip";

        // Create a temporary folder to extract the archive to
        var tempArchiveFolder = PathExtensions.GetTempFileName();

        try
        {
            await using var uploadedStream = model.Content.OpenReadStream();
            await using var scannedStream = await CreateScannedStreamAsync(model.Content.FileName, model.Content.ContentType, uploadedStream);

            using (var fs = System.IO.File.Create(tempArchiveName))
            {
                await scannedStream.CopyToAsync(fs);
            }

            ZipFile.ExtractToDirectory(tempArchiveName, tempArchiveFolder);

            await _deploymentManager.ImportDeploymentPackageAsync(new PhysicalFileProvider(tempArchiveFolder));
        }
        catch (RecipeExecutionException e)
        {
            _logger.LogError(e, "Unable to import a recipe from deployment plan.");

            await _notifier.ErrorAsync(H["The deployment plan failed with the following errors: {0}", string.Join(' ', e.StepResult.Errors)]);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unexpected error occurred while executing a deployment plan.");

            await _notifier.ErrorAsync(H["Unexpected error occurred while executing a deployment plan."]);
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

    private async Task<Stream> CreateScannedStreamAsync(string fileName, string contentType, Stream stream)
    {
        if (_fileUploadScanner is NullAntivirusScanner)
        {
            return stream;
        }

        var scanStream = stream;

        if (scanStream.CanSeek)
        {
            scanStream.Position = 0;
        }
        else
        {
            scanStream = await CreateTemporarySeekableStreamAsync(scanStream);
        }

        try
        {
            var result = await _fileUploadScanner.ScanAsync(
                new AntivirusScanContext(fileName, scanStream.Length, contentType),
                scanStream);

            if (!result.IsClean)
            {
                throw new AntivirusScanningException(result.Message ?? $"The uploaded file '{fileName}' was rejected by the anti-virus scanner.");
            }

            scanStream.Position = 0;

            return scanStream;
        }
        catch
        {
            if (scanStream != stream)
            {
                scanStream.Dispose();
            }

            throw;
        }
    }

    private static async Task<Stream> CreateTemporarySeekableStreamAsync(Stream stream)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempStream = new FileStream(
            tempFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None,
            81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        await stream.CopyToAsync(tempStream);
        tempStream.Position = 0;

        return tempStream;
    }
}

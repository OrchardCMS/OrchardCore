using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Media.Services;

public sealed class MediaConfigurationValidator : ModularTenantEvents
{
    private readonly IOptions<MediaOptions> _mediaOptions;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public MediaConfigurationValidator(
        IOptions<MediaOptions> mediaOptions,
        IWebHostEnvironment hostEnvironment,
        ShellSettings shellSettings,
        ILogger<MediaConfigurationValidator> logger)
    {
        _mediaOptions = mediaOptions;
        _hostEnvironment = hostEnvironment;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public override Task ActivatedAsync()
    {
        if (_shellSettings.IsRunning())
        {
            CheckIisConfiguration();
        }

        return Task.CompletedTask;
    }

    private void CheckIisConfiguration()
    {
        var webConfigPath = Path.Combine(_hostEnvironment.ContentRootPath, "web.config");

        if (!File.Exists(webConfigPath))
        {
            return;
        }

        try
        {
            var doc = XDocument.Load(webConfigPath);
            var requestLimits = doc.Descendants("requestLimits").FirstOrDefault();
            var maxAllowedContentLengthAttr = requestLimits?.Attribute("maxAllowedContentLength");

            long iisLimit;

            if (maxAllowedContentLengthAttr == null || !long.TryParse(maxAllowedContentLengthAttr.Value, out iisLimit))
            {
                // IIS default is 30,000,000 bytes when not explicitly configured.
                iisLimit = 30_000_000;
            }

            var maxFileSize = _mediaOptions.Value.MaxFileSize;

            if (iisLimit < maxFileSize)
            {
                _logger.LogWarning(
                    "The IIS maxAllowedContentLength ({IISLimit} bytes) is lower than " +
                    "MediaOptions.MaxFileSize ({MaxFileSize} bytes). Uploads larger than {IISLimit} bytes " +
                    "will be rejected by IIS before reaching the application. " +
                    "Update the requestLimits in web.config or reduce MediaOptions.MaxFileSize.",
                    iisLimit,
                    maxFileSize,
                    iisLimit);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not parse web.config to check IIS request limits.");
        }
    }
}

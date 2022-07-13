using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;
using SixLabors.ImageSharp.Web.Caching;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Media.Removing
{
    /// <summary>
    /// Used to remove the 'ImageSharp' cache folder of a given tenant on shell removing.
    /// </summary>
    public class ShellMediaCacheRemovingHandler : ModularTenantEvents
    {
        private readonly IWebHostEnvironment _environment;
        private readonly PhysicalFileSystemCacheOptions _options;
        private readonly ILogger _logger;

        public ShellMediaCacheRemovingHandler(
            IWebHostEnvironment environment,
            IOptions<PhysicalFileSystemCacheOptions> options,
            ILogger<ShellMediaCacheRemovingHandler> logger)
        {
            _environment = environment;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Removes the 'ImageSharp' cache folder while removing the provided tenant.
        /// </summary>
        public override Task RemovingAsync(ShellRemovingContext context)
        {
            var cacheFolder = Path.Combine(
                _options.CacheRootPath ?? _environment.WebRootPath,
                _options.CacheFolder);

            try
            {
                Directory.Delete(cacheFolder, true);
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to remove the 'ImageSharp' cache folder '{CacheFolder}' of tenant '{TenantName}'.",
                    cacheFolder,
                    context.ShellSettings.Name);

                context.ErrorMessage = $"Failed to remove the 'ImageSharp' cache folder '{cacheFolder}'.";
                context.Error = ex;
            }

            return Task.CompletedTask;
        }
    }
}

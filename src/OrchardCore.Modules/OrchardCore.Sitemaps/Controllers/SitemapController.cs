using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Controllers
{
    public class SitemapController : Controller
    {
        private const int WarningLength = 47_185_920;
        private const int ErrorLength = 52_428_800;

        private static readonly ConcurrentDictionary<string, Lazy<Task<Stream>>> _workers = new(StringComparer.OrdinalIgnoreCase);

        private readonly ISitemapManager _sitemapManager;
        private readonly ISiteService _siteService;
        private readonly ISitemapBuilder _sitemapBuilder;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly string _tenantName;
        private readonly ILogger _logger;

        public SitemapController(
            ISitemapManager sitemapManager,
            ISiteService siteService,
            ISitemapBuilder sitemapBuilder,
            ISitemapCacheProvider sitemapCacheProvider,
            ShellSettings shellSettings,
            ILogger<SitemapController> logger
            )
        {
            _sitemapManager = sitemapManager;
            _siteService = siteService;
            _sitemapBuilder = sitemapBuilder;
            _sitemapCacheProvider = sitemapCacheProvider;
            _tenantName = shellSettings.Name;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string sitemapId, CancellationToken cancellationToken)
        {
            var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);
            if (sitemap == null || !sitemap.Enabled)
            {
                return NotFound();
            }

            var fileResolver = await _sitemapCacheProvider.GetCachedSitemapAsync(sitemap.CacheFileName);
            if (fileResolver != null)
            {
                // When multiple requests occur for the same sitemap it 
                // may still be building, so we wait for it to complete.
                if (_workers.TryGetValue(_tenantName + sitemap.Path, out var writeTask))
                {
                    await writeTask.Value;
                }

                // File result will dispose of stream.
                var stream = await fileResolver.OpenReadStreamAsync();
                return File(stream, "application/xml");
            }
            else
            {
                var work = await _workers.GetOrAdd(_tenantName + sitemap.Path, x => new Lazy<Task<Stream>>(async () =>
                {
                    try
                    {
                        var siteSettings = await _siteService.GetSiteSettingsAsync();

                        var context = new SitemapBuilderContext()
                        {
                            HostPrefix = siteSettings.BaseUrl,
                            UrlHelper = Url
                        };

                        var document = await _sitemapBuilder.BuildAsync(sitemap, context);

                        if (document == null)
                        {
                            return null;
                        }

                        document.Declaration = new XDeclaration("1.0", "utf-8", null);

                        var stream = new MemoryStream();
                        await document.SaveAsync(stream, SaveOptions.None, cancellationToken);

                        if (stream.Length >= ErrorLength)
                        {
                            _logger.LogError("Sitemap 50MB maximum length limit exceeded");
                        }
                        else if (stream.Length >= WarningLength)
                        {
                            _logger.LogWarning("Sitemap nearing 50MB length limit");
                        }

                        await _sitemapCacheProvider.SetSitemapCacheAsync(stream, sitemap.CacheFileName, cancellationToken);

                        return stream;
                    }
                    finally
                    {
                        _workers.TryRemove(_tenantName + sitemap.Path, out var writeCacheTask);
                    }
                }, LazyThreadSafetyMode.ExecutionAndPublication)).Value;

                if (work == null)
                {
                    return NotFound();
                }

                work.Position = 0;

                // File result will dispose of stream.
                return File(work, "application/xml");
            }
        }
    }
}

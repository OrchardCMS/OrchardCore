using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps
{
    public class DefaultSitemapManager : ISitemapManager
    {
        private readonly IEnumerable<ISitemapProvider> _sitemapProviders;
        private readonly ISiteService _siteService;
        private readonly IUrlHelper _urlHelper;

        public DefaultSitemapManager(
            ILogger<DefaultSitemapManager> logger,
            IEnumerable<ISitemapProvider> sitemapProviders,
            ISiteService siteService,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory)
        {
            Logger = logger;
            _sitemapProviders = sitemapProviders;
            _siteService = siteService;
            _urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()));
        }

        public ILogger<DefaultSitemapManager> Logger { get; }

        public async Task<object> BuildSitemap(int? number)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var sitemapsSettings = siteSettings.As<SitemapsSettings>();

            var entries = new List<SitemapEntry>();

            foreach(var sitemapProvider in _sitemapProviders)
            {
                entries.AddRange(await sitemapProvider.BuildSitemapEntries());
            }
            
            var pageSize = sitemapsSettings.MaxEntriesPerSitemap == 0 ? 50000 : sitemapsSettings.MaxEntriesPerSitemap;

            entries = entries.Distinct(new SitemapEntryComparer()).ToList();

            if (entries.Count > pageSize)
            {
                // Do we need to return the sitemap index?
                if (number == 0)
                {
                    var siteMapCount = (int)Math.Round(((float)entries.Count / (float)pageSize) + 0.5f);
                    var sitemapIndex = new SitemapIndex();
                    //TODO _urlHelper.Action being properly weird here - index contains no value errors all the time - it'll be me doing something stupid no doubt...
                    //tried with IActionContextAccessor but still no success, so have gone back to a plain new actioncontext as it means not requiring the singleton
                    //sitemapIndex.Items.AddRange(Enumerable.Range(0, siteMapCount).Select(x => new SitemapIndexItem()
                    //{
                    //    Location = _urlHelper.ToAbsoluteAction("Index", "Sitemaps", new { area = "OrchardCore.Sitemaps", number = x + 1 }),
                    //    LastModifiedUtc = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                    //}));
                    sitemapIndex.Items.AddRange(Enumerable.Range(0, siteMapCount).Select(x => new SitemapIndexItem(){
                        Location = _urlHelper.GetBaseUrl() + $"/sitemap{x + 1}.xml",
                        LastModifiedUtc = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")
                    }));
                    return sitemapIndex;
                    //return new XElement(Namespace + "sitemapindex", siteMapEntries.Select(ToSitemapElement));
                }

                // Take a slice of the entire sitemap.
                var skip = (number.Value -1) * pageSize;
                var take = pageSize;

                entries = entries.Skip(skip).Take(take).ToList();
            }
            
            var sitemapUrlset = new SitemapUrlset();
            sitemapUrlset.Items.AddRange(entries.Select(x => new SitemapUrlItem()
            {
                Location = x.Url,
                ChangeFrequency = x.ChangeFrequency.HasValue ? x.ChangeFrequency.ToString().ToLower() : null,
                Priority = x.Priority ?? (float?)null,
                LastModified = x.LastModifiedUtc.HasValue ? x.LastModifiedUtc.Value.ToString("yyyy-MM-ddTHH:mm:sszzz") : null
            }));

            return sitemapUrlset;
        }
    }
}

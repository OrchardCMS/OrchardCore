using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.Autoroute.Model;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Sitemaps.Models;
using YesSql;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using OrchardCore.Sitemaps;
using OrchardCore.Modules;

namespace OrchardCore.Autoroute.Sitemaps
{
    [Feature("OrchardCore.Sitemaps")]
    public class AutorouteSitemapProvider : ISitemapProvider
    {
        private readonly YesSql.ISession _session;
        private readonly IUrlHelper _urlHelper;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AutorouteSitemapProvider(
            ILogger<AutorouteSitemapProvider> logger,
            YesSql.ISession session,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelperFactory urlHelperFactory,
            IContentDefinitionManager contentDefinitionManager)
        {
            Logger = logger;
            _session = session;
            _urlHelper = urlHelperFactory.GetUrlHelper(new ActionContext(httpContextAccessor.HttpContext, new RouteData(), new ActionDescriptor()));
            _contentDefinitionManager = contentDefinitionManager;
        }

        public ILogger<AutorouteSitemapProvider> Logger { get; }

        public async Task<IList<SitemapEntry>> BuildSitemapEntries()
        {
            var sitemapEntries = new List<SitemapEntry>();

            var items = await _session
                .Query<ContentItem, AutoroutePartIndex>(x => x.Published)
                .ListAsync();

            foreach(var item in items)
            {
                var autoroutePart = item.As<AutoroutePart>();
                var sitemapPart = item.As<SitemapPart>();
                
                var exclude = sitemapPart != null ? sitemapPart.Exclude : false;
                if (!exclude)
                {
                    var sitemapPartSettings = GetSettings(sitemapPart);
                    exclude = sitemapPartSettings != null ? sitemapPartSettings.ExcludePart : false;
                }
                if (exclude)
                    continue;

                var changeFrequency = sitemapPart != null ? sitemapPart.ChangeFrequency : ChangeFrequency.Daily;
                var priority = sitemapPart != null ? sitemapPart.Priority : 0.5f;
                //TODO this should probably recieved a context with a url helper and build from Content
                //var contentItemMetadata = await ContentManager.PopulateAspectAsync<ContentItemMetadata>(item);
                //var relativeUrl = Url.RouteUrl(contentItemMetadata.DisplayRouteValues);
                //var absoluteUrl = Url.ToAbsoluteUrl(relativeUrl);
                string path = autoroutePart.Path;
                if (!String.IsNullOrEmpty(path))
                {
                    path = path.StartsWith("/") ? path : path.Insert(0, "/");
                    path = path.StartsWith("~/") ? path.Replace("~", String.Empty) : path;
                }

                var sitemapEntry = new SitemapEntry()
                {
                    Url = _urlHelper.ToAbsoluteUrl(path),
                    ChangeFrequency = changeFrequency,
                    //ChangeFrequency = changeFrequency.ToString().ToLower(),
                    Priority = priority,
                    //LastModified = item.ModifiedUtc.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz")
                    LastModifiedUtc = item.ModifiedUtc
            };
                sitemapEntries.Add(sitemapEntry);
            }
            return sitemapEntries;
        }

        private SitemapPartSettings GetSettings(SitemapPart sitemapPart)
        {
            if (sitemapPart == null)
                return null;

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(sitemapPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(SitemapPart), StringComparison.Ordinal));
            return contentTypePartDefinition.Settings.ToObject<SitemapPartSettings>();
        }
    }
}

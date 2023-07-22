using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Builders
{
    public class CustomPathSitemapSourceBuilder : SitemapSourceBuilderBase<CustomPathSitemapSource>
    {
        private static readonly XNamespace _namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        public override async Task BuildSourceAsync(CustomPathSitemapSource source, SitemapBuilderContext context)
        {
            var url = new XElement(_namespace + "url");

            if (await BuildUrlsetMetadataAsync(source, context, url))
            {
                context.Response.ResponseElement.Add(url);
            }
        }

        private static Task<bool> BuildUrlsetMetadataAsync(CustomPathSitemapSource source, SitemapBuilderContext context, XElement url)
        {
            if (BuildUrl(context, source, url))
            {
                PopulateLastMod(source, url);
                PopulateChangeFrequencyPriority(source, url);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        private static bool BuildUrl(SitemapBuilderContext context, CustomPathSitemapSource source, XElement url)
        {
            if (String.IsNullOrEmpty(source.Path))
                return false;

            // Add ~/ to the path, because the it is inserted without leading /.
            var path = "~/" + source.Path;

            var loc = new XElement(_namespace + "loc");
            loc.Add(context.HostPrefix + context.UrlHelper.Content(path));
            url.Add(loc);
            return true;
        }

        private static void PopulateChangeFrequencyPriority(CustomPathSitemapSource source, XElement url)
        {
            var changeFrequencyValue = source.ChangeFrequency.ToString();
            var priorityIntValue = source.Priority;

            var priorityValue = (priorityIntValue * 0.1f).ToString(CultureInfo.InvariantCulture);

            var changeFreq = new XElement(_namespace + "changefreq");
            changeFreq.Add(changeFrequencyValue.ToLower());
            url.Add(changeFreq);

            var priority = new XElement(_namespace + "priority");
            priority.Add(priorityValue);
            url.Add(priority);
        }

        private static void PopulateLastMod(CustomPathSitemapSource source, XElement url)
        {
            // Last modified is not required. Do not include if the path has no modified date.
            if (source.LastUpdate.HasValue)
            {
                var lastMod = new XElement(_namespace + "lastmod");
                lastMod.Add(source.LastUpdate.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
                url.Add(lastMod);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps.Services
{
    public interface ISitemapNodeProviderFactory
    {
        string Name { get; }
        SitemapNode Create(SitemapSet sitemapSet);
    }

    public class SitemapNodeProviderFactory<TSitemapNode> : ISitemapNodeProviderFactory where TSitemapNode : SitemapNode, new()
    {
        private static readonly string TypeName = typeof(TSitemapNode).Name;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;

        public string Name => TypeName;

        public SitemapNodeProviderFactory(ISitemapIdGenerator sitemapIdGenerator)
        {
            _sitemapIdGenerator = sitemapIdGenerator;
        }

        public SitemapNode Create(SitemapSet sitemapSet)
        {
            return new TSitemapNode()
            {
                Id = _sitemapIdGenerator.GenerateUniqueId(),
                SitemapSet = sitemapSet
            };
        }
    }
}

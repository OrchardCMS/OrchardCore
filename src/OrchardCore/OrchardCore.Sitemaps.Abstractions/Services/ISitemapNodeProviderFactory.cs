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
        SitemapNode Create();
    }

    public class SitemapNodeProviderFactory<TSitemapNode> : ISitemapNodeProviderFactory where TSitemapNode : SitemapNode, new()
    {
        private static readonly string TypeName = typeof(TSitemapNode).Name;

        public string Name => TypeName;

        public SitemapNode Create()
        {
            return new TSitemapNode();
        }
    }
}

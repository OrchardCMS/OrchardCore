using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Sitemaps.Models
{
    /// <summary>
    /// The document of all SitemapSets stored on the system.
    /// </summary>
    public class SitemapDocument
    {
        public int Id { get; set; }
        public IList<SitemapSet> SitemapSets { get; set; } = new List<SitemapSet>();

        public void SetNodeSet()
        {
            foreach (var set in SitemapSets)
            {
                RecurseNodes(set, set.SitemapNodes);
            }
        }

        private void RecurseNodes(SitemapSet set, IList<SitemapNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.SitemapSet = set;
                RecurseNodes(set, node.ChildNodes);
            }
        }
    }

    public static class SitemapDocumentExtensions
    {
        public static SitemapSet GetSitemapSetById(this SitemapDocument document, string id)
        {
            return document.SitemapSets.FirstOrDefault(x => x.Id == id);
        }

        public static SitemapNode GetSitemapNodeById(this SitemapDocument document, string sitemapNodeId)
        {
            foreach (var sitemapSet in document.SitemapSets)
            {
                var sitemapNode = sitemapSet.GetSitemapNodeById(sitemapNodeId);
                if (sitemapNode != null)
                    return sitemapNode;
            }
            return null;
        }
    }

}

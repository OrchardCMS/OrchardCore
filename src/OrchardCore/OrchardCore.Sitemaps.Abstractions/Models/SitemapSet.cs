using System;
using System.Collections.Generic;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapSet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public List<SitemapNode> SitemapNodes { get; } = new List<SitemapNode>();
    }

    public static class SitemapSetExtensions
    {
        public static SitemapNode GetSitemapNodeById(this SitemapSet sitemapSet, string id)
        {
            foreach (var sitemapNode in sitemapSet.SitemapNodes)
            {
                var found = sitemapNode.GetSitemapNodeById(id, sitemapSet);
                if (found != null)
                {
                    return found;
                }
            }

            // not found
            return null;
        }

        public static bool RemoveSitemapNode(this SitemapSet sitemapSet, SitemapNode sitemapNodeToRemove)
        {
            if (sitemapSet.SitemapNodes.Contains(sitemapNodeToRemove))
            {
                sitemapSet.SitemapNodes.Remove(sitemapNodeToRemove);
                return true;
            }
            else
            {
                foreach (var firstLevelSitemapNode in sitemapSet.SitemapNodes)
                {
                    if (firstLevelSitemapNode.RemoveSitemapNode(sitemapNodeToRemove))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool InsertSitemapNodeAt(this SitemapSet sitemapSet, SitemapNode sitemapNodeToInsert, SitemapNode destinationSitemapNode, int position)
        {
            if (sitemapNodeToInsert == null)
            {
                throw new ArgumentNullException("sitemapNodeToInsert");
            }

            // insert the node at the destination node
            if (destinationSitemapNode == null)
            {
                sitemapSet.SitemapNodes.Insert(position, sitemapNodeToInsert);
                return true;
            }
            else
            {
                foreach (var firstLevelSitemapNode in sitemapSet.SitemapNodes)
                {
                    if (firstLevelSitemapNode.InsertSitemapNode(sitemapNodeToInsert, destinationSitemapNode, position))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

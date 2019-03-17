using System;
using System.Collections.Generic;
using OrchardCore.Navigation;


namespace OrchardCore.Sitemaps.Models
{
    public class SitemapSet
    {
        public string Id { get; set; } 
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public string RootPath { get; set; } 
        public List<SitemapNode> SitemapNodes { get; } = new List<SitemapNode>();


        public SitemapNode GetSitemapNodeById(string id)
        {
            foreach (var sitemapNode in SitemapNodes)
            {
                var found = sitemapNode.GetSitemapNodeById(id, this);
                if (found != null)
                {
                    return found;
                }
            }

            // not found
            return null;
        }

        public bool RemoveSitemapNode(SitemapNode sitemapNodeToRemove)
        {
            if (SitemapNodes.Contains(sitemapNodeToRemove)) // todo: avoid this check by having a single TreeNode as a property of the content tree preset.
            {
                SitemapNodes.Remove(sitemapNodeToRemove);
                return true; 
            }
            else
            {
                foreach (var firstLevelSitemapNode in SitemapNodes)
                {
                    if (firstLevelSitemapNode.RemoveSitemapNode(sitemapNodeToRemove))
                    {
                        return true; 
                    }
                }                
            }

            return false; 
        }

        public bool InsertSitemapNodeAt(SitemapNode sitemapNodeToInsert, SitemapNode destinationSitemapNode, int position)
        {
            if (sitemapNodeToInsert == null)
            {
                throw new ArgumentNullException("sitemapNodeToInsert");
            }

            // insert the node at the destination node
            if (destinationSitemapNode == null)
            {
                SitemapNodes.Insert(position, sitemapNodeToInsert);
                return true; 
            }
            else
            {
                foreach (var firstLevelSitemapNode in SitemapNodes)
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

using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Navigation;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapNode : MenuItem
    {
        public string UniqueId { get; set; } = Guid.NewGuid().ToString("n");
        public bool Enabled { get; set; } = true;        
        
        public SitemapNode GetMenuItemById(string id)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { this });

            while (tempStack.Any())
            {
                // evaluate first node
                SitemapNode item = tempStack.Pop();
                if (item.UniqueId.Equals(id, StringComparison.OrdinalIgnoreCase)) return item;

                // not that one; continue with the rest.
                foreach (var i in item.Items) tempStack.Push((SitemapNode)i);
            }

            //not found
            return null;
        }

        // return boolean so that caller can check for success
        public bool RemoveMenuItem(SitemapNode nodeToRemove)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { this });

            while (tempStack.Any())
            {
                // evaluate first
                MenuItem item = tempStack.Pop();
                if (item.Items.Contains(nodeToRemove))
                {
                    item.Items.Remove(nodeToRemove);
                    return true; //success
                }

                // not that one. continue
                foreach (var i in item.Items) tempStack.Push((SitemapNode)i);
            }

            // failure
            return false;
        }


        public bool InsertMenuItem(SitemapNode nodeToInsert, MenuItem destinationNode, int position)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { this });
            while (tempStack.Any())
            {
                // evaluate first
                MenuItem node = tempStack.Pop();
                if (node.Equals(destinationNode))
                {
                    node.Items.Insert(position, nodeToInsert);
                    return true; // success
                }

                // not that one. continue
                foreach (var n in node.Items) tempStack.Push((SitemapNode)n);
            }

            // failure
            return false;
        }
    }
}

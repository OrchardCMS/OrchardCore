using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace OrchardCore.Sitemaps.Models
{
    public class SitemapNode
    {
        public string Id { get; set; }
        public bool Enabled { get; set; } = true;

        [Required]
        public string Path { get; set; }

        [JsonIgnore]
        public SitemapSet SitemapSet { get; set; }

        // With the nodes currently implemented we don't need both these options, one would suffice, however
        // this allows room to might plug in more nodes below a parent to support google news or other metadata related.

        /// <summary>
        /// Override to disable support for child nodes
        /// </summary>
        [JsonIgnore]
        public virtual bool CanSupportChildNodes => true;

        /// <summary>
        /// Override to force this Node to the root only
        /// </summary>
        [JsonIgnore]
        public virtual bool CanBeChildNode => true;

        /// <summary>
        /// The child nodes.
        /// </summary>
        public List<SitemapNode> ChildNodes { get; set; } = new List<SitemapNode>();
    }

    public static class SitemapNodeExtensions
    {
        public static SitemapNode GetSitemapNodeById(this SitemapNode sitemapNode, string id, SitemapSet sitemapSet)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { sitemapNode });

            while (tempStack.Any())
            {
                // evaluate first node
                var item = tempStack.Pop();
                if (item.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    item.SitemapSet = sitemapSet;
                    return item;
                }

                // not that one; continue with the rest.
                foreach (var i in item.ChildNodes) tempStack.Push(i);
            }

            //not found
            return null;
        }

        public static bool RemoveSitemapNode(this SitemapNode sitemapNode, SitemapNode nodeToRemove)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { sitemapNode });

            while (tempStack.Any())
            {
                // evaluate first
                var item = tempStack.Pop();
                if (item.ChildNodes.Contains(nodeToRemove))
                {
                    item.ChildNodes.Remove(nodeToRemove);
                    return true; //success
                }

                // not that one. continue
                foreach (var i in item.ChildNodes) tempStack.Push(i);
            }

            // failure
            return false;
        }

        public static bool InsertSitemapNode(this SitemapNode sitemapNode, SitemapNode nodeToInsert, SitemapNode destinationNode, int position)
        {
            var tempStack = new Stack<SitemapNode>(new SitemapNode[] { sitemapNode });
            while (tempStack.Any())
            {
                // evaluate first
                var node = tempStack.Pop();
                if (node.Equals(destinationNode))
                {
                    node.ChildNodes.Insert(position, nodeToInsert);
                    return true; // success
                }

                // not that one. continue
                foreach (var n in node.ChildNodes) tempStack.Push(n);
            }

            // failure
            return false;
        }
    }
}

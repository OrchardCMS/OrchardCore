using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace OrchardCore.Feeds.Models
{
    public class FeedResponse
    {
        public XElement Element { get; set; }
        public IList<FeedItem> Items { get; } = new List<FeedItem>();
        public IList<Action<ContextualizeContext>> Contextualizers { get; } = new List<Action<ContextualizeContext>>();
        public void Contextualize(Action<ContextualizeContext> contextualizer)
        {
            Contextualizers.Add(contextualizer);
        }
    }
}

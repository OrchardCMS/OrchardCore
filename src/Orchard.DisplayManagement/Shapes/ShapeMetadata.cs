using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNet.Html.Abstractions;

namespace Orchard.DisplayManagement.Shapes
{
    public class ShapeMetadata
    {
        public ShapeMetadata()
        {
            Wrappers = new List<string>();
            Alternates = new List<string>();
            BindingSources = new List<string>();
            Displaying = Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayedContext>>();
        }

        public string Type { get; set; }
        public string DisplayType { get; set; }
        public string Position { get; set; }
        public string Tab { get; set; }
        public string PlacementSource { get; set; }
        public string Prefix { get; set; }
        public IList<string> Wrappers { get; set; }
        public IList<string> Alternates { get; set; }
        public ShapeMetadataCacheContext CacheContext => new ShapeMetadataCacheContext();

        public bool WasExecuted { get; set; }
        public IHtmlContent ChildContent { get; set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Action<ShapeDisplayingContext>> Displaying { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Action<ShapeDisplayedContext>> Displayed { get; private set; }

        public IList<string> BindingSources { get; set; }

        public void OnDisplaying(Action<ShapeDisplayingContext> action)
        {
            var existing = Displaying ?? Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Displaying = existing.Concat(new[] { action });
        }

        public void OnDisplayed(Action<ShapeDisplayedContext> action)
        {
            var existing = Displayed ?? Enumerable.Empty<Action<ShapeDisplayedContext>>();
            Displayed = existing.Concat(new[] { action });
        }

        public class ShapeMetadataCacheContext
        {
            public string CacheId { get; set; }
            public HashSet<string> Contexts => new HashSet<string>();
            public HashSet<string> Tags => new HashSet<string>();
            public int Duration => int.MaxValue;
        }
    }

    
}
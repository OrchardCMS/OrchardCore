using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNetCore.Html;
using System.Threading.Tasks;
using Orchard.Environment.Cache;

namespace Orchard.DisplayManagement.Shapes
{
    public class ShapeMetadata
    {
        private CacheContext _cacheContext;

        public ShapeMetadata()
        {
            Wrappers = new List<string>();
            Alternates = new List<string>();
            BindingSources = new List<string>();
            Displaying = Enumerable.Empty<Action<ShapeDisplayingContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayedContext>>();
            Processing = Enumerable.Empty<Func<dynamic, Task>>();
        }

        public string Type { get; set; }
        public string DisplayType { get; set; }
        public string Position { get; set; }
        public string Tab { get; set; }
        public string PlacementSource { get; set; }
        public string Prefix { get; set; }
        public IList<string> Wrappers { get; set; }
        public IList<string> Alternates { get; set; }
        public bool IsCached => _cacheContext != null;
        public bool WasExecuted { get; set; }
        public IHtmlContent ChildContent { get; set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Action<ShapeDisplayingContext>> Displaying { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Func<dynamic, Task>> Processing { get; private set; }

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

        public void OnProcessing(Func<dynamic, Task> processing)
        {
            var existing = Processing ?? Enumerable.Empty<Func<dynamic, Task>>();
            Processing = existing.Concat(new[] { processing });
        }

        public void OnDisplayed(Action<ShapeDisplayedContext> action)
        {
            var existing = Displayed ?? Enumerable.Empty<Action<ShapeDisplayedContext>>();
            Displayed = existing.Concat(new[] { action });
        }

        /// <summary>
        /// Marks this shape to be cached
        /// </summary>
        public CacheContext Cache(string cacheId)
        {
            if(_cacheContext == null || _cacheContext.CacheId != cacheId)
            {
                _cacheContext = new CacheContext(cacheId);
            }

            return _cacheContext;
        }

        /// <summary>
        /// Returns the <see cref="ShapeMetadataCacheContext"/> instance if the shape is cached.
        /// </summary>
        public CacheContext Cache()
        {
            return _cacheContext;
        }
    }

}
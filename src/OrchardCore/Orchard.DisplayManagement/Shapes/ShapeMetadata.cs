using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Orchard.DisplayManagement.Implementation;
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
            Displaying = Enumerable.Empty<Action<ShapeDisplayContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayContext>>();
            ProcessingAsync = Enumerable.Empty<Func<dynamic, Task>>();
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
        public IEnumerable<Action<ShapeDisplayContext>> Displaying { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Func<dynamic, Task>> ProcessingAsync { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        public IEnumerable<Action<ShapeDisplayContext>> Displayed { get; private set; }


        public IList<string> BindingSources { get; set; }

        public void OnDisplaying(Action<ShapeDisplayContext> context)
        {
            Displaying = Displaying.Concat(new[] { context });
        }

        public void OnProcessing(Func<dynamic, Task> context)
        {
            ProcessingAsync = ProcessingAsync.Concat(new[] { context });
        }

        public void OnDisplayed(Action<ShapeDisplayContext> context)
        {
            Displayed = Displayed.Concat(new[] { context });
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
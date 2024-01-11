using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DisplayManagement.Shapes
{
    public class ShapeMetadata
    {
        private CacheContext _cacheContext;

        public ShapeMetadata()
        {
            Wrappers = new AlternatesCollection();
            Alternates = new AlternatesCollection();
            BindingSources = new List<string>();
            Displaying = Enumerable.Empty<Action<ShapeDisplayContext>>();
            Displayed = Enumerable.Empty<Action<ShapeDisplayContext>>();
            ProcessingAsync = Enumerable.Empty<Func<dynamic, Task>>();
        }

        public string Type { get; set; }
        public string DisplayType { get; set; }
        public string Position { get; set; }
        public string Tab { get; set; }
        public string Card { get; set; }
        public string Column { get; set; }
        public string PlacementSource { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Differentiator { get; set; }
        public AlternatesCollection Wrappers { get; set; }
        public AlternatesCollection Alternates { get; set; }
        public bool IsCached => _cacheContext != null;
        public IHtmlContent ChildContent { get; set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Action<ShapeDisplayContext>> Displaying { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Func<IShape, Task>> ProcessingAsync { get; private set; }

        /// <summary>
        /// Event use for a specific shape instance.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<Action<ShapeDisplayContext>> Displayed { get; private set; }

        [JsonIgnore]
        public IList<string> BindingSources { get; set; }

        public void OnDisplaying(Action<ShapeDisplayContext> context)
        {
            Displaying = Displaying.Concat(new[] { context });
        }

        public void OnProcessing(Func<IShape, Task> context)
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
            if (_cacheContext == null || _cacheContext.CacheId != cacheId)
            {
                _cacheContext = new CacheContext(cacheId);
            }

            return _cacheContext;
        }

        /// <summary>
        /// Returns the <see cref="CacheContext"/> instance if the shape is cached.
        /// </summary>
        public CacheContext Cache()
        {
            return _cacheContext;
        }
    }
}

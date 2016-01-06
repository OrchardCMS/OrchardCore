using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNet.Html.Abstractions;

namespace Orchard.DisplayManagement.Shapes
{
    public class ShapeMetadata
    {
        private ShapeMetadataCacheContext _cacheContext;

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

        /// <summary>
        /// Marks this shape to be cached
        /// </summary>
        public ShapeMetadataCacheContext Cache(string cacheId)
        {
            _cacheContext = new ShapeMetadataCacheContext(cacheId);
            return _cacheContext;
        }

        /// <summary>
        /// Returns the <see cref="ShapeMetadataCacheContext"/> instance if the shape is cached.
        /// </summary>
        public ShapeMetadataCacheContext Cache()
        {
            return _cacheContext;
        }

    }

    public class ShapeMetadataCacheContext
    {
        private HashSet<string> _contexts;
        private HashSet<string> _tags;
        private string _cacheId;
        private TimeSpan? _duration;

        public ShapeMetadataCacheContext(string cacheId)
        {
            _cacheId = cacheId;
        }

        /// <summary>
        /// Defines the absolute time the shape should be cached for.
        /// If not called a sliding value will be used.
        /// </summary>
        public ShapeMetadataCacheContext During(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }
        
        /// <summary>
        /// Defines a dimension to cache the shape for. For instance by using <code>"user"</code>
        /// each user will get a different value.
        /// </summary>
        public ShapeMetadataCacheContext AddContext(params string[] contexts)
        {
            if(_contexts == null)
            {
                _contexts = new HashSet<string>();
            }

            foreach (var context in contexts)
            {
                _contexts.Add(context);
            }

            return this;
        }

        /// <summary>
        /// Removes a specific context.
        /// </summary>
        public ShapeMetadataCacheContext RemoveContext(string context)
        {
            if(_contexts != null)
            {
                _contexts.Remove(context);
            }

            return this; 
        }

        /// <summary>
        /// Defines a dimension that will invalidate the cache entry when it changes.
        /// For instance by using <code>"features"</code> every time the list of features
        /// will change the value of the cache will be invalidated.
        /// </summary>
        public ShapeMetadataCacheContext AddDependency(params string[] context)
        {
            return AddContext(context);
        }

        /// <summary>
        /// Removes a specific dependency.
        /// </summary>
        public ShapeMetadataCacheContext RemoveDependency(string context)
        {
            return RemoveContext(context);
        }

        public ShapeMetadataCacheContext AddTag(params string[] tags)
        {
            if (_tags == null)
            {
                _tags = new HashSet<string>();
            }

            foreach (var tag in tags)
            {
                _tags.Add(tag);
            }

            return this;
        }

        public ShapeMetadataCacheContext RemoveTag(string tag)
        {
            if (_tags != null)
            {
                _tags.Remove(tag);
            }

            return this;
        }

        public string CacheId => _cacheId;
        public IEnumerable<string> Contexts => _contexts ?? Enumerable.Empty<string>();
        public IEnumerable<string> Tags => _tags ?? Enumerable.Empty<string>();
        public TimeSpan? Duration => _duration;

    }
}
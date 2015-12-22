using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Implementation;
using Microsoft.AspNet.Html.Abstractions;
using Orchard.Environment.Cache.Abstractions;

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

            _cacheContext = new ShapeMetadataCacheContext();
        }

        public string Type { get; set; }
        public string DisplayType { get; set; }
        public string Position { get; set; }
        public string Tab { get; set; }
        public string PlacementSource { get; set; }
        public string Prefix { get; set; }
        public IList<string> Wrappers { get; set; }
        public IList<string> Alternates { get; set; }
        public ShapeMetadataCacheContext CacheContext => _cacheContext;

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

    }

    public class ShapeMetadataCacheContext
    {
        private HashSet<string> _contexts;
        private List<CacheContextEntry> _dependencies;
        private string _cacheId;
        private TimeSpan? _duration;

        public ShapeMetadataCacheContext Cache(string cacheId)
        {
            _cacheId = cacheId;
            return this;
        }

        public ShapeMetadataCacheContext During(TimeSpan duration)
        {
            _duration = duration;
            return this;
        }
        
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

        public ShapeMetadataCacheContext RemoveContext(string context)
        {
            if(_contexts != null)
            {
                _contexts.Remove(context);
            }

            return this; 
        }

        public ShapeMetadataCacheContext AddDependency(string context, string value)
        {
            if (_dependencies == null)
            {
                _dependencies = new List<CacheContextEntry>();
            }

            _dependencies.Add(new CacheContextEntry(context, value));

            return this;
        }

        public ShapeMetadataCacheContext RemoveDependency(string context, string value)
        {
            if (_dependencies != null)
            {
                _dependencies.RemoveAll(x => x.Key == context && x.Value == value);
            }

            return this;
        }

        public ShapeMetadataCacheContext RemoveDependency(string context)
        {
            if (_dependencies != null)
            {
                _dependencies.RemoveAll(x => x.Key == context);
            }

            return this;
        }

        public string CacheId => _cacheId;
        public IEnumerable<string> Contexts => _contexts ?? Enumerable.Empty<string>();
        public IEnumerable<CacheContextEntry> Dependencies => _dependencies ?? Enumerable.Empty<CacheContextEntry>();
        public TimeSpan? Duration => _duration;

    }
}
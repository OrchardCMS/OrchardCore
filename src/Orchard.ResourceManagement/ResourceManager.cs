using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.ResourceManagement
{
    public class ResourceManager : IResourceManager
    {
        private readonly Dictionary<Tuple<String, String>, RequireSettings> _required = new Dictionary<Tuple<String, String>, RequireSettings>();
        private readonly List<LinkEntry> _links = new List<LinkEntry>();
        private readonly Dictionary<string, MetaEntry> _metas = new Dictionary<string, MetaEntry> {
            { "generator", new MetaEntry { Content = "Orchard", Name = "generator" } }
        };
        private readonly Dictionary<string, IList<ResourceRequiredContext>> _builtResources = new Dictionary<string, IList<ResourceRequiredContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly IEnumerable<IResourceManifestProvider> _providers;
        private ResourceManifest _dynamicManifest;
        private List<String> _headScripts;
        private List<String> _footScripts;


        private const string NotIE = "!IE";

        private static string ToAppRelativePath(string resourcePath)
        {
            if (!String.IsNullOrEmpty(resourcePath) && !Uri.IsWellFormedUriString(resourcePath, UriKind.Absolute))
            {
                resourcePath = VirtualPathUtility.ToAppRelative(resourcePath);
            }
            return resourcePath;
        }

        private static string FixPath(string resourcePath, string relativeFromPath)
        {
            if (!String.IsNullOrEmpty(resourcePath) && !VirtualPathUtility.IsAbsolute(resourcePath) && !Uri.IsWellFormedUriString(resourcePath, UriKind.Absolute))
            {
                // appears to be a relative path (e.g. 'foo.js' or '../foo.js', not "/foo.js" or "http://..")
                if (String.IsNullOrEmpty(relativeFromPath))
                {
                    throw new InvalidOperationException("ResourcePath cannot be relative unless a base relative path is also provided.");
                }
                resourcePath = VirtualPathUtility.ToAbsolute(VirtualPathUtility.Combine(relativeFromPath, resourcePath));
            }
            return resourcePath;
        }

        private static TagBuilder GetTagBuilder(ResourceDefinition resource, string url)
        {
            var tagBuilder = new TagBuilder(resource.TagName);
            tagBuilder.MergeAttributes(resource.TagBuilder.Attributes);
            if (!String.IsNullOrEmpty(resource.FilePathAttributeName))
            {
                if (!String.IsNullOrEmpty(url))
                {
                    if (VirtualPathUtility.IsAppRelative(url))
                    {
                        url = VirtualPathUtility.ToAbsolute(url);
                    }
                    tagBuilder.MergeAttribute(resource.FilePathAttributeName, url, true);
                }
            }
            return tagBuilder;
        }

        public static void WriteResource(TextWriter writer, ResourceDefinition resource, string url, string condition, Dictionary<string, string> attributes)
        {
            if (!string.IsNullOrEmpty(condition))
            {
                if (condition == NotIE)
                {
                    writer.WriteLine("<!--[if " + condition + "]>-->");
                }
                else
                {
                    writer.WriteLine("<!--[if " + condition + "]>");
                }
            }

            var tagBuilder = GetTagBuilder(resource, url);

            if (attributes != null)
            {
                // todo: try null value
                tagBuilder.MergeAttributes(attributes, true);
            }

            writer.WriteLine(tagBuilder.ToString(resource.TagRenderMode));

            if (!string.IsNullOrEmpty(condition))
            {
                if (condition == NotIE)
                {
                    writer.WriteLine("<!--<![endif]-->");
                }
                else
                {
                    writer.WriteLine("<![endif]-->");
                }
            }
        }

        private readonly IResourceManifestState _resourceManifestState;

        public ResourceManager(
            IEnumerable<IResourceManifestProvider> resourceProviders,
            IResourceManifestState resourceManifestState)
        {
            _resourceManifestState = resourceManifestState;
            _providers = resourceProviders;
        }

        public IEnumerable<IResourceManifest> ResourceProviders
        {
            get
            {
                if (_resourceManifestState.Manifest == null)
                {
                    var builder = new ResourceManifestBuilder();
                    foreach (var provider in _providers)
                    {
                        builder.Feature = provider.Metadata.ContainsKey("Feature") ?
                            (Feature)provider.Metadata["Feature"] :
                            null;
                        provider.Value.BuildManifests(builder);
                    }
                    _resourceManifestState.Manifest = builder.ResourceManifests;
                }
                return _resourceManifestState.Manifest;
            }
        }

        public virtual ResourceManifest DynamicResources
        {
            get
            {
                return _dynamicManifest ?? (_dynamicManifest = new ResourceManifest());
            }
        }

        public virtual RequireSettings Require(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException("resourceType");
            }
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }
            RequireSettings settings;
            var key = new Tuple<string, string>(resourceType, resourceName);
            if (!_required.TryGetValue(key, out settings))
            {
                settings = new RequireSettings { Type = resourceType, Name = resourceName };
                _required[key] = settings;
            }
            _builtResources[resourceType] = null;
            return settings;
        }

        public virtual RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath)
        {
            return Include(resourceType, resourcePath, resourceDebugPath, null);
        }

        public virtual RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException("resourceType");
            }
            if (resourcePath == null)
            {
                throw new ArgumentNullException("resourcePath");
            }

            // ~/ ==> convert to absolute path (e.g. /orchard/..)
            if (VirtualPathUtility.IsAppRelative(resourcePath))
            {
                resourcePath = VirtualPathUtility.ToAbsolute(resourcePath);
            }
            if (resourceDebugPath != null && VirtualPathUtility.IsAppRelative(resourceDebugPath))
            {
                resourceDebugPath = VirtualPathUtility.ToAbsolute(resourceDebugPath);
            }

            resourcePath = FixPath(resourcePath, relativeFromPath);
            resourceDebugPath = FixPath(resourceDebugPath, relativeFromPath);
            return Require(resourceType, ToAppRelativePath(resourcePath)).Define(d => d.SetUrl(resourcePath, resourceDebugPath));
        }

        public virtual void RegisterHeadScript(string script)
        {
            if (_headScripts == null)
            {
                _headScripts = new List<string>();
            }
            _headScripts.Add(script);
        }

        public virtual void RegisterFootScript(string script)
        {
            if (_footScripts == null)
            {
                _footScripts = new List<string>();
            }
            _footScripts.Add(script);
        }

        public virtual void NotRequired(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException("resourceType");
            }
            if (resourceName == null)
            {
                throw new ArgumentNullException("resourceName");
            }
            var key = new Tuple<string, string>(resourceType, resourceName);
            _builtResources[resourceType] = null;
            _required.Remove(key);
        }

        public virtual ResourceDefinition FindResource(RequireSettings settings)
        {
            return FindResource(settings, true);
        }

        private ResourceDefinition FindResource(RequireSettings settings, bool resolveInlineDefinitions)
        {
            // find the resource with the given type and name
            // that has at least the given version number. If multiple,
            // return the resource with the greatest version number.
            // If not found and an inlineDefinition is given, define the resource on the fly
            // using the action.
            var name = settings.Name ?? "";
            var type = settings.Type;
            var resource = (from p in ResourceProviders
                            from r in p.GetResources(type)
                            where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                            let version = r.Value.Version != null ? new Version(r.Value.Version) : null
                            orderby version descending
                            select r.Value).FirstOrDefault();
            if (resource == null && _dynamicManifest != null)
            {
                resource = (from r in _dynamicManifest.GetResources(type)
                            where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                            let version = r.Value.Version != null ? new Version(r.Value.Version) : null
                            orderby version descending
                            select r.Value).FirstOrDefault();
            }
            if (resolveInlineDefinitions && resource == null)
            {
                // Does not seem to exist, but it's possible it is being
                // defined by a Define() from a RequireSettings somewhere.
                if (ResolveInlineDefinitions(settings.Type))
                {
                    // if any were defined, now try to find it
                    resource = FindResource(settings, false);
                }
            }
            return resource;
        }

        private bool ResolveInlineDefinitions(string resourceType)
        {
            bool anyWereDefined = false;
            foreach (var settings in GetRequiredResources(resourceType).Where(settings => settings.InlineDefinition != null))
            {
                // defining it on the fly
                var resource = FindResource(settings, false);
                if (resource == null)
                {
                    // does not already exist, so define it
                    resource = DynamicResources.DefineResource(resourceType, settings.Name).SetBasePath(settings.BasePath);
                    anyWereDefined = true;
                }
                settings.InlineDefinition(resource);
                settings.InlineDefinition = null;
            }
            return anyWereDefined;
        }

        public virtual IEnumerable<RequireSettings> GetRequiredResources(string type)
        {
            return _required.Where(r => r.Key.Item1 == type).Select(r => r.Value);
        }

        public virtual IList<LinkEntry> GetRegisteredLinks()
        {
            return _links.AsReadOnly();
        }

        public virtual IList<MetaEntry> GetRegisteredMetas()
        {
            return _metas.Values.ToList().AsReadOnly();
        }

        public virtual IList<String> GetRegisteredHeadScripts()
        {
            return _headScripts == null ? null : _headScripts.AsReadOnly();
        }

        public virtual IList<String> GetRegisteredFootScripts()
        {
            return _footScripts == null ? null : _footScripts.AsReadOnly();
        }

        public virtual IList<ResourceRequiredContext> BuildRequiredResources(string resourceType)
        {
            IList<ResourceRequiredContext> requiredResources;
            if (_builtResources.TryGetValue(resourceType, out requiredResources) && requiredResources != null)
            {
                return requiredResources;
            }
            var allResources = new OrderedDictionary();
            foreach (var settings in GetRequiredResources(resourceType))
            {
                var resource = FindResource(settings);
                if (resource == null)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "A '{1}' named '{0}' could not be found.", settings.Name, settings.Type));
                }
                ExpandDependencies(resource, settings, allResources);
            }
            requiredResources = (from DictionaryEntry entry in allResources
                                 select new ResourceRequiredContext { Resource = (ResourceDefinition)entry.Key, Settings = (RequireSettings)entry.Value }).ToList();
            _builtResources[resourceType] = requiredResources;
            return requiredResources;
        }

        protected virtual void ExpandDependencies(ResourceDefinition resource, RequireSettings settings, OrderedDictionary allResources)
        {
            if (resource == null)
            {
                return;
            }
            // Settings is given so they can cascade down into dependencies. For example, if Foo depends on Bar, and Foo's required
            // location is Head, so too should Bar's location.
            // forge the effective require settings for this resource
            // (1) If a require exists for the resource, combine with it. Last settings in gets preference for its specified values.
            // (2) If no require already exists, form a new settings object based on the given one but with its own type/name.
            settings = allResources.Contains(resource)
                ? ((RequireSettings)allResources[resource]).Combine(settings)
                : new RequireSettings { Type = resource.Type, Name = resource.Name }.Combine(settings);
            if (resource.Dependencies != null)
            {
                var dependencies = from d in resource.Dependencies
                                   select FindResource(new RequireSettings { Type = resource.Type, Name = d });
                foreach (var dependency in dependencies)
                {
                    if (dependency == null)
                    {
                        continue;
                    }
                    ExpandDependencies(dependency, settings, allResources);
                }
            }
            allResources[resource] = settings;
        }

        public void RegisterLink(LinkEntry link)
        {
            _links.Add(link);
        }

        public void SetMeta(MetaEntry meta)
        {
            if (meta == null)
            {
                return;
            }

            var index = meta.Name ?? meta.HttpEquiv ?? "charset";

            _metas[index] = meta;
        }

        public void AppendMeta(MetaEntry meta, string contentSeparator)
        {
            if (meta == null)
            {
                return;
            }

            var index = meta.Name ?? meta.HttpEquiv;

            if (String.IsNullOrEmpty(index))
            {
                return;
            }

            MetaEntry existingMeta;
            if (_metas.TryGetValue(index, out existingMeta))
            {
                meta = MetaEntry.Combine(existingMeta, meta, contentSeparator);
            }
            _metas[index] = meta;
        }

    }
}

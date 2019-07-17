using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace OrchardCore.ResourceManagement
{
    public class ResourceManager : IResourceManager
    {
        private readonly Dictionary<ResourceTypeName, RequireSettings> _required = new Dictionary<ResourceTypeName, RequireSettings>();
        private readonly Dictionary<string, IList<ResourceRequiredContext>> _builtResources;
        private readonly string _pathBase;
        private readonly IEnumerable<IResourceManifestProvider> _providers;
        private readonly IFileVersionProvider _fileVersionProvider;
        private ResourceManifest _dynamicManifest;

        private List<LinkEntry> _links;
        private Dictionary<string, MetaEntry> _metas;
        private List<IHtmlContent> _headScripts;
        private List<IHtmlContent> _footScripts;
        private HashSet<string> _localScripts;

        private readonly IResourceManifestState _resourceManifestState;
        private readonly IOptions<ResourceManagementOptions> _options;

        public ResourceManager(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IResourceManifestProvider> resourceProviders,
            IResourceManifestState resourceManifestState,
            IOptions<ResourceManagementOptions> options,
            IFileVersionProvider fileVersionProvider)
        {
            _resourceManifestState = resourceManifestState;
            _options = options;
            _pathBase = httpContextAccessor.HttpContext.Request.PathBase;
            _providers = resourceProviders;
            _fileVersionProvider = fileVersionProvider;

            _builtResources = new Dictionary<string, IList<ResourceRequiredContext>>(StringComparer.OrdinalIgnoreCase);
            _localScripts = new HashSet<string>();
        }

        public IEnumerable<ResourceManifest> ResourceManifests
        {
            get
            {
                if (_resourceManifestState.ResourceManifests == null)
                {
                    var builder = new ResourceManifestBuilder();
                    foreach (var provider in _providers)
                    {
                        provider.BuildManifests(builder);
                    }
                    _resourceManifestState.ResourceManifests = builder.ResourceManifests;
                }
                return _resourceManifestState.ResourceManifests;
            }
        }

        public ResourceManifest InlineManifest
        {
            get
            {
                if (_dynamicManifest == null)
                {
                    _dynamicManifest = new ResourceManifest();
                }

                return _dynamicManifest;
            }
        }

        public RequireSettings RegisterResource(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            RequireSettings settings;
            var key = new ResourceTypeName(resourceType, resourceName);
            if (!_required.TryGetValue(key, out settings))
            {
                settings = new RequireSettings(_options.Value) { Type = resourceType, Name = resourceName };
                _required[key] = settings;
            }
            _builtResources[resourceType] = null;
            return settings;
        }

        public RequireSettings Include(string resourceType, string resourcePath, string resourceDebugPath)
        {
            return RegisterUrl(resourceType, resourcePath, resourceDebugPath, null);
        }

        public RequireSettings RegisterUrl(string resourceType, string resourcePath, string resourceDebugPath, string relativeFromPath)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (resourcePath == null)
            {
                throw new ArgumentNullException(nameof(resourcePath));
            }

            // ~/ ==> convert to absolute path (e.g. /orchard/..)

            if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourcePath = _pathBase + resourcePath.Substring(1);
            }

            if (resourceDebugPath != null && resourceDebugPath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourceDebugPath = _pathBase + resourceDebugPath.Substring(1);
            }

            return RegisterResource(resourceType, GetResourceKey(resourcePath, resourceDebugPath)).Define(d => d.SetUrl(resourcePath, resourceDebugPath));
        }

        public void RegisterHeadScript(IHtmlContent script)
        {
            if (_headScripts == null)
            {
                _headScripts = new List<IHtmlContent>();
            }

            _headScripts.Add(script);
        }

        public void RegisterFootScript(IHtmlContent script)
        {
            if (_footScripts == null)
            {
                _footScripts = new List<IHtmlContent>();
            }

            _footScripts.Add(script);
        }

        public void NotRequired(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                throw new ArgumentNullException(nameof(resourceType));
            }

            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var key = new ResourceTypeName(resourceType, resourceName);
            _builtResources[resourceType] = null;
            _required.Remove(key);
        }

        public ResourceDefinition FindResource(RequireSettings settings)
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
            ResourceDefinition resource;

            var resources = (from p in ResourceManifests
                             from r in p.GetResources(type)
                             where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                             select r.Value).SelectMany(x => x);

            if (!String.IsNullOrEmpty(settings.Version))
            {
                // Specific version, filter
                var upper = GetUpperBoundVersion(settings.Version);
                var lower = GetLowerBoundVersion(settings.Version);
                resources = from r in resources
                            let version = r.Version != null ? new Version(r.Version) : null
                            where lower <= version && version < upper
                            select r;
            }

            // Use the highest version of all matches
            resource = (from r in resources
                        let version = r.Version != null ? new Version(r.Version) : null
                        orderby version descending
                        select r).FirstOrDefault();

            if (resource == null && _dynamicManifest != null)
            {
                resources = (from r in _dynamicManifest.GetResources(type)
                             where name.Equals(r.Key, StringComparison.OrdinalIgnoreCase)
                             select r.Value).SelectMany(x => x);

                if (!String.IsNullOrEmpty(settings.Version))
                {
                    // Specific version, filter
                    var upper = GetUpperBoundVersion(settings.Version);
                    var lower = GetLowerBoundVersion(settings.Version);
                    resources = from r in resources
                                let version = r.Version != null ? new Version(r.Version) : null
                                where lower <= version && version < upper
                                select r;
                }

                // Use the highest version of all matches
                resource = (from r in resources
                            let version = r.Version != null ? new Version(r.Version) : null
                            orderby version descending
                            select r).FirstOrDefault();
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

        /// <summary>
        /// Returns the upper bound value of a required version number.
        /// For instance, 3.1.0 returns 3.1.1, 4 returns 5.0.0, 6.1 returns 6.2.0 
        /// </summary>
        private Version GetUpperBoundVersion(string minimumVersion)
        {
            Version version;

            if (!Version.TryParse(minimumVersion, out version))
            {
                // Is is a single number?
                int major;
                if (int.TryParse(minimumVersion, out major))
                {
                    return new Version(major + 1, 0, 0);
                }
            }

            if (version.Build != -1)
            {
                return new Version(version.Major, version.Minor, version.Build + 1);
            }

            if (version.Minor != -1)
            {
                return new Version(version.Major, version.Minor + 1, 0);
            }

            return version;
        }

        /// <summary>
        /// Returns the lower bound value of a required version number.
        /// For instance, 3.1.0 returns 3.1.0, 4 returns 4.0.0, 6.1 returns 6.1.0 
        /// </summary>
        private Version GetLowerBoundVersion(string minimumVersion)
        {
            Version version;

            if (!Version.TryParse(minimumVersion, out version))
            {
                // Is is a single number?
                int major;
                if (int.TryParse(minimumVersion, out major))
                {
                    return new Version(major, 0, 0);
                }
            }

            return version;
        }

        private bool ResolveInlineDefinitions(string resourceType)
        {
            bool anyWereDefined = false;
            foreach (var settings in ResolveRequiredResources(resourceType).Where(settings => settings.InlineDefinition != null))
            {
                // defining it on the fly
                var resource = FindResource(settings, false);
                if (resource == null)
                {
                    // does not already exist, so define it
                    resource = InlineManifest.DefineResource(resourceType, settings.Name).SetBasePath(settings.BasePath);
                    anyWereDefined = true;
                }
                settings.InlineDefinition(resource);
                settings.InlineDefinition = null;
            }
            return anyWereDefined;
        }

        private IEnumerable<RequireSettings> ResolveRequiredResources(string resourceType)
        {
            return _required.Where(r => r.Key.Type == resourceType).Select(r => r.Value);
        }

        public IEnumerable<LinkEntry> GetRegisteredLinks()
        {
            if (_links == null)
            {
                return Enumerable.Empty<LinkEntry>();
            }

            return _links.AsReadOnly();
        }

        public IEnumerable<MetaEntry> GetRegisteredMetas()
        {
            if (_metas == null)
            {
                return Enumerable.Empty<MetaEntry>();
            }

            return _metas.Values;
        }

        public IEnumerable<IHtmlContent> GetRegisteredHeadScripts()
        {
            return _headScripts == null ? Enumerable.Empty<IHtmlContent>() : _headScripts;
        }

        public IEnumerable<IHtmlContent> GetRegisteredFootScripts()
        {
            return _footScripts == null ? Enumerable.Empty<IHtmlContent>() : _footScripts;
        }

        public IEnumerable<ResourceRequiredContext> GetRequiredResources(string resourceType)
        {
            IList<ResourceRequiredContext> requiredResources;
            if (_builtResources.TryGetValue(resourceType, out requiredResources) && requiredResources != null)
            {
                return requiredResources;
            }
            var allResources = new OrderedDictionary();
            foreach (var settings in ResolveRequiredResources(resourceType))
            {
                var resource = FindResource(settings);
                if (resource == null)
                {
                    throw new InvalidOperationException($"Could not find a resource of type '{settings.Type}' named '{settings.Name}' with version '{settings.Version ?? "any"}.");
                }
                ExpandDependencies(resource, settings, allResources);
            }
            requiredResources = (from DictionaryEntry entry in allResources
                                 select new ResourceRequiredContext {
                                     Resource = (ResourceDefinition)entry.Key,
                                     Settings = (RequireSettings)entry.Value,
                                     FileVersionProvider = _fileVersionProvider
                                 }).ToList();
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
                : new RequireSettings(_options.Value) { Type = resource.Type, Name = resource.Name }.Combine(settings);
            if (resource.Dependencies != null)
            {
                var dependencies = from d in resource.Dependencies
                                   let segments = d.Split(':')
                                   let name = segments[0]
                                   let version = segments.Length > 1 ? segments[1] : null
                                   select FindResource(new RequireSettings { Type = resource.Type, Name = name, Version = version });
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
            if (_links == null)
            {
                _links = new List<LinkEntry>();
            }

            var href = link.Href;

            if (href != null && href.StartsWith("~/", StringComparison.Ordinal))
            {
                link.Href = _pathBase + href.Substring(1);
            }

            if (link.AppendVersion)
            {
                link.Href = _fileVersionProvider.AddFileVersionToPath(_pathBase, link.Href);
            }

            _links.Add(link);
        }

        public void RegisterMeta(MetaEntry meta)
        {
            if (meta == null)
            {
                return;
            }

            if (_metas == null)
            {
                _metas = new Dictionary<string, MetaEntry>();
            }

            var index = meta.Name ?? meta.Property ?? meta.HttpEquiv ?? "charset";

            _metas[index] = meta;
        }

        public void AppendMeta(MetaEntry meta, string contentSeparator)
        {
            if (meta == null)
            {
                return;
            }

            var index = meta.Name ?? meta.Property ?? meta.HttpEquiv;

            if (String.IsNullOrEmpty(index))
            {
                return;
            }

            if (_metas == null)
            {
                _metas = new Dictionary<string, MetaEntry>();
            }

            MetaEntry existingMeta;
            if (_metas.TryGetValue(index, out existingMeta))
            {
                meta = MetaEntry.Combine(existingMeta, meta, contentSeparator);
            }

            _metas[index] = meta;
        }

        public void RenderMeta(IHtmlContentBuilder builder)
        {
            var first = true;

            foreach (var meta in this.GetRegisteredMetas())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(meta.GetTag());
            }
        }

        public void RenderHeadLink(IHtmlContentBuilder builder)
        {
            var first = true;

            foreach (var link in this.GetRegisteredLinks())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(link.GetTag());
            }
        }

        public void RenderStylesheet(IHtmlContentBuilder builder)
        {
            var first = true;

            var styleSheets = this.GetRequiredResources("stylesheet");

            foreach (var context in styleSheets)
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_pathBase));
            }
        }

        public void RenderHeadScript(IHtmlContentBuilder builder)
        {
            var headScripts = this.GetRequiredResources("script");

            var first = true;

            foreach (var context in headScripts.Where(r => r.Settings.Location == ResourceLocation.Head))
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_pathBase));
            }

            foreach (var context in GetRegisteredHeadScripts())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

        public void RenderFootScript(IHtmlContentBuilder builder)
        {
            var footScripts = this.GetRequiredResources("script");

            var first = true;

            foreach (var context in footScripts.Where(r => r.Settings.Location == ResourceLocation.Foot))
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_pathBase));
            }

            foreach (var context in GetRegisteredFootScripts())
            {
                if (!first)
                {
                    builder.AppendHtml(Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

        public void RenderLocalScript(RequireSettings settings, IHtmlContentBuilder builder)
        {
            var localScripts = this.GetRequiredResources("script");

            var first = true;

            foreach (var context in localScripts.Where(r => r.Settings.Location == ResourceLocation.Unspecified))
            {
                if (_localScripts.Add(context.Settings.Name) || context.Settings.Name == settings.Name)
                {
                    if (!first)
                    {
                        builder.AppendHtml(Environment.NewLine);
                    }

                    first = false;

                    builder.AppendHtml(context.GetHtmlContent(_pathBase));
                }
            }
        }

        private class ResourceTypeName : IEquatable<ResourceTypeName>
        {
            public string Type { get; }
            public string Name { get; }

            public ResourceTypeName(string resourceType, string resourceName)
            {
                Type = resourceType;
                Name = resourceName;
            }

            public bool Equals(ResourceTypeName other)
            {
                if (other == null)
                {
                    return false;
                }

                return Type.Equals(other.Type) && Name.Equals(other.Name);
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode() << 17 + Name.GetHashCode();
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("(");
                sb.Append(Type);
                sb.Append(", ");
                sb.Append(Name);
                sb.Append(")");
                return sb.ToString();
            }
        }

        private string GetResourceKey(string releasePath, string debugPath)
        {
            if (_options.Value.DebugMode && !string.IsNullOrWhiteSpace(debugPath))
            {
                return debugPath;
            }
            else
            {
                return releasePath;
            }
        }
    }
}

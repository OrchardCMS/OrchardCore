using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace OrchardCore.ResourceManagement
{
    public class ResourceManager : IResourceManager
    {
        private readonly Dictionary<ResourceTypeName, RequireSettings> _required = new Dictionary<ResourceTypeName, RequireSettings>();
        private readonly Dictionary<string, ResourceRequiredContext[]> _builtResources;
        private readonly IEnumerable<IResourceManifestProvider> _providers;
        private readonly IFileVersionProvider _fileVersionProvider;
        private ResourceManifest _dynamicManifest;

        private List<LinkEntry> _links;
        private Dictionary<string, MetaEntry> _metas;
        private List<IHtmlContent> _headScripts;
        private List<IHtmlContent> _footScripts;
        private List<IHtmlContent> _styles;
        private readonly HashSet<string> _localScripts;

        private readonly IResourceManifestState _resourceManifestState;
        private readonly ResourceManagementOptions _options;

        public ResourceManager(
            IEnumerable<IResourceManifestProvider> resourceProviders,
            IResourceManifestState resourceManifestState,
            IOptions<ResourceManagementOptions> options,
            IFileVersionProvider fileVersionProvider)
        {
            _resourceManifestState = resourceManifestState;
            _options = options.Value;
            _providers = resourceProviders;
            _fileVersionProvider = fileVersionProvider;

            _builtResources = new Dictionary<string, ResourceRequiredContext[]>(StringComparer.OrdinalIgnoreCase);
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

        public ResourceManifest InlineManifest => _dynamicManifest ?? (_dynamicManifest = new ResourceManifest());

        public RequireSettings RegisterResource(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                return ThrowArgumentNullException<RequireSettings>(nameof(resourceType));
            }

            if (resourceName == null)
            {
                return ThrowArgumentNullException<RequireSettings>(nameof(resourceName));
            }

            var key = new ResourceTypeName(resourceType, resourceName);
            if (!_required.TryGetValue(key, out var settings))
            {
                settings = new RequireSettings(_options)
                {
                    Type = resourceType,
                    Name = resourceName
                };
                _required[key] = settings;
            }
            _builtResources[resourceType] = null;
            return settings;
        }

        public RequireSettings RegisterUrl(string resourceType, string resourcePath, string resourceDebugPath)
        {
            if (resourceType == null)
            {
                return ThrowArgumentNullException<RequireSettings>(nameof(resourceType));
            }

            if (resourcePath == null)
            {
                return ThrowArgumentNullException<RequireSettings>(nameof(resourcePath));
            }

            // ~/ ==> convert to absolute path (e.g. /orchard/..)

            if (resourcePath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourcePath = _options.ContentBasePath + resourcePath.Substring(1);
            }

            if (resourceDebugPath != null && resourceDebugPath.StartsWith("~/", StringComparison.Ordinal))
            {
                resourceDebugPath = _options.ContentBasePath + resourceDebugPath.Substring(1);
            }

            return RegisterResource(
                resourceType,
                GetResourceKey(resourcePath, resourceDebugPath)).Define(d => d.SetUrl(resourcePath, resourceDebugPath));
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

        public void RegisterStyle(IHtmlContent style)
        {
            if (_styles == null)
            {
                _styles = new List<IHtmlContent>();
            }

            _styles.Add(style);
        }

        public void NotRequired(string resourceType, string resourceName)
        {
            if (resourceType == null)
            {
                ThrowArgumentNullException(nameof(resourceType));
                return;
            }

            if (resourceName == null)
            {
                ThrowArgumentNullException(nameof(resourceName));
                return;
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

            var stream = ResourceManifests.SelectMany(x => x.GetResources(type));
            var resource = FindMatchingResource(stream, settings, name);

            if (resource == null && _dynamicManifest != null)
            {
                stream = _dynamicManifest.GetResources(type);
                resource = FindMatchingResource(stream, settings, name);
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

        private ResourceDefinition FindMatchingResource(
            IEnumerable<KeyValuePair<string, IList<ResourceDefinition>>> stream,
            RequireSettings settings,
            string name)
        {
            Version lower = null;
            Version upper = null;
            if (!String.IsNullOrEmpty(settings.Version))
            {
                // Specific version, filter
                lower = GetLowerBoundVersion(settings.Version);
                upper = GetUpperBoundVersion(settings.Version);
            }

            ResourceDefinition resource = null;
            foreach (var r in stream)
            {
                if (String.Equals(r.Key, name, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var resourceDefinition in r.Value)
                    {
                        var version = resourceDefinition.Version != null
                            ? new Version(resourceDefinition.Version)
                            : null;

                        if (lower != null)
                        {
                            if (lower > version || version >= upper)
                            {
                                continue;
                            }
                        }

                        // Use the highest version of all matches
                        if (resource == null
                            || (resourceDefinition.Version != null && new Version(resource.Version) < version))
                        {
                            resource = resourceDefinition;
                        }
                    }
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
            if (!Version.TryParse(minimumVersion, out var version))
            {
                // Is is a single number?
                if (int.TryParse(minimumVersion, out var major))
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
            if (!Version.TryParse(minimumVersion, out var version))
            {
                // Is is a single number?
                if (int.TryParse(minimumVersion, out var major))
                {
                    return new Version(major, 0, 0);
                }
            }

            return version;
        }

        private bool ResolveInlineDefinitions(string resourceType)
        {
            bool anyWereDefined = false;
            foreach (var settings in ResolveRequiredResources(resourceType))
            {
                if (settings.InlineDefinition == null)
                {
                    continue;
                }

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
            foreach (var (key, value) in _required)
            {
                if (key.Type == resourceType)
                {
                    yield return value;
                }
            }
        }

        public IEnumerable<LinkEntry> GetRegisteredLinks() => DoGetRegisteredLinks();

        private List<LinkEntry> DoGetRegisteredLinks()
        {
            return _links ?? EmptyList<LinkEntry>.Instance;
        }

        public IEnumerable<MetaEntry> GetRegisteredMetas() => DoGetRegisteredMetas();

        private Dictionary<string, MetaEntry>.ValueCollection DoGetRegisteredMetas()
        {
            return _metas?.Values ?? EmptyValueCollection<MetaEntry>.Instance;
        }

        public IEnumerable<IHtmlContent> GetRegisteredHeadScripts() => DoGetRegisteredHeadScripts();

        public List<IHtmlContent> DoGetRegisteredHeadScripts()
        {
            return _headScripts ?? EmptyList<IHtmlContent>.Instance;
        }

        public IEnumerable<IHtmlContent> GetRegisteredFootScripts() => DoGetRegisteredFootScripts();

        public List<IHtmlContent> DoGetRegisteredFootScripts()
        {
            return _footScripts ?? EmptyList<IHtmlContent>.Instance;
        }

        public IEnumerable<IHtmlContent> GetRegisteredStyles() => DoGetRegisteredStyles();

        public List<IHtmlContent> DoGetRegisteredStyles()
        {
            return _styles ?? EmptyList<IHtmlContent>.Instance;
        }

        public IEnumerable<ResourceRequiredContext> GetRequiredResources(string resourceType)
            => DoGetRequiredResources(resourceType);

        private ResourceRequiredContext[] DoGetRequiredResources(string resourceType)
        {
            if (_builtResources.TryGetValue(resourceType, out var requiredResources) && requiredResources != null)
            {
                return requiredResources;
            }

            var allResources = new OrderedDictionary();
            foreach (var settings in ResolveRequiredResources(resourceType))
            {
                var resource = FindResource(settings);
                if (resource == null)
                {
                    throw new InvalidOperationException($"Could not find a resource of type '{settings.Type}' named '{settings.Name}' with version '{settings.Version ?? "any"}'.");
                }

                // Register any additional dependencies for the resource here,
                // rather than in Combine as they are additive, and should not be Combined.
                if (settings.Dependencies != null)
                {
                    resource.SetDependencies(settings.Dependencies);
                }

                ExpandDependencies(resource, settings, allResources);
            }

            requiredResources = new ResourceRequiredContext[allResources.Count];
            var i = 0;
            foreach (DictionaryEntry entry in allResources)
            {
                requiredResources[i++] = new ResourceRequiredContext
                {
                    Resource = (ResourceDefinition)entry.Key,
                    Settings = (RequireSettings)entry.Value,
                    FileVersionProvider = _fileVersionProvider
                };
            }

            _builtResources[resourceType] = requiredResources;
            return requiredResources;
        }

        protected virtual void ExpandDependencies(
            ResourceDefinition resource,
            RequireSettings settings,
            OrderedDictionary allResources)
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
                : new RequireSettings(_options) { Type = resource.Type, Name = resource.Name }.Combine(settings);

            if (resource.Dependencies != null)
            {
                // share search instance
                var tempSettings = new RequireSettings();

                for (var i = 0; i < resource.Dependencies.Count; i++)
                {
                    var d = resource.Dependencies[i];
                    var idx = d.IndexOf(':');
                    var name = d;
                    string version = null;
                    if (idx != -1)
                    {
                        name = d.Substring(0, idx);
                        version = d.Substring(idx + 1);
                    }

                    tempSettings.Type = resource.Type;
                    tempSettings.Name = name;
                    tempSettings.Version = version;

                    var dependency = FindResource(tempSettings);
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
                link.Href = _options.ContentBasePath + href.Substring(1);
            }

            if (link.AppendVersion)
            {
                link.Href = _fileVersionProvider.AddFileVersionToPath(_options.ContentBasePath, link.Href);
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

            if (_metas.TryGetValue(index, out var existingMeta))
            {
                meta = MetaEntry.Combine(existingMeta, meta, contentSeparator);
            }

            _metas[index] = meta;
        }

        public void RenderMeta(IHtmlContentBuilder builder)
        {
            var first = true;

            foreach (var meta in DoGetRegisteredMetas())
            {
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(meta.GetTag());
            }
        }

        public void RenderHeadLink(IHtmlContentBuilder builder)
        {
            var first = true;

            var registeredLinks = DoGetRegisteredLinks();
            for (var i = 0; i < registeredLinks.Count; i++)
            {
                var link = registeredLinks[i];
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(link.GetTag());
            }
        }

        public void RenderStylesheet(IHtmlContentBuilder builder)
        {
            var first = true;

            var styleSheets = DoGetRequiredResources("stylesheet");

            foreach (var context in styleSheets)
            {
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_options.ContentBasePath));
            }

            var registeredStyles = DoGetRegisteredStyles();
            for (var i = 0; i < registeredStyles.Count; i++)
            {
                var context = registeredStyles[i];
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

        public void RenderHeadScript(IHtmlContentBuilder builder)
        {
            var headScripts = DoGetRequiredResources("script");

            var first = true;

            foreach (var context in headScripts)
            {
                if (context.Settings.Location != ResourceLocation.Head)
                {
                    continue;
                }

                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_options.ContentBasePath));
            }

            var registeredHeadScripts = DoGetRegisteredHeadScripts();
            for (var i = 0; i < registeredHeadScripts.Count; i++)
            {
                var context = registeredHeadScripts[i];
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

        public void RenderFootScript(IHtmlContentBuilder builder)
        {
            var footScripts = DoGetRequiredResources("script");

            var first = true;
            foreach (var context in footScripts)
            {
                if (context.Settings.Location != ResourceLocation.Foot)
                {
                    continue;
                }

                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context.GetHtmlContent(_options.ContentBasePath));
            }

            var registeredFootScripts = DoGetRegisteredFootScripts();
            for (var i = 0; i < registeredFootScripts.Count; i++)
            {
                var context = registeredFootScripts[i];
                if (!first)
                {
                    builder.AppendHtml(System.Environment.NewLine);
                }

                first = false;

                builder.AppendHtml(context);
            }
        }

        public void RenderLocalScript(RequireSettings settings, IHtmlContentBuilder builder)
        {
            var localScripts = DoGetRequiredResources("script");

            var first = true;

            foreach (var context in localScripts)
            {
                if (context.Settings.Location == ResourceLocation.Unspecified
                    && (_localScripts.Add(context.Settings.Name) || context.Settings.Name == settings.Name))
                {
                    if (!first)
                    {
                        builder.AppendHtml(System.Environment.NewLine);
                    }

                    first = false;

                    builder.AppendHtml(context.GetHtmlContent(_options.ContentBasePath));
                }
            }
        }

        private readonly struct ResourceTypeName : IEquatable<ResourceTypeName>
        {
            public readonly string Type;
            public readonly string Name;

            public ResourceTypeName(string resourceType, string resourceName)
            {
                Type = resourceType;
                Name = resourceName;
            }

            public bool Equals(ResourceTypeName other)
            {
                return Type == other.Type && Name == other.Name;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Type, Name);
            }

            public override string ToString() => "(" + Type + ", " + Name + ")";
        }

        private string GetResourceKey(string releasePath, string debugPath)
        {
            if (_options.DebugMode && !string.IsNullOrWhiteSpace(debugPath))
            {
                return debugPath;
            }
            else
            {
                return releasePath;
            }
        }

        private static class EmptyList<T>
        {
            public static readonly List<T> Instance = new List<T>();
        }

        private static class EmptyValueCollection<T>
        {
            public static readonly Dictionary<string, T>.ValueCollection Instance = new Dictionary<string, T>.ValueCollection(new Dictionary<string, T>());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException(string paramName)
        {
            ThrowArgumentNullException<object>(paramName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T ThrowArgumentNullException<T>(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}

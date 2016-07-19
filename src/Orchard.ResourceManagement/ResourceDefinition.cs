using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Orchard.ResourceManagement
{
    public class ResourceDefinition
    {
        private static readonly Dictionary<string, string> _resourceTypeTagNames = new Dictionary<string, string> {
            { "script", "script" },
            { "stylesheet", "link" }
        };
        private static readonly Dictionary<string, string> _filePathAttributes = new Dictionary<string, string> {
            { "script", "src" },
            { "link", "href" }
        };
        private static readonly Dictionary<string, Dictionary<string, string>> _resourceAttributes = new Dictionary<string, Dictionary<string, string>> {
            { "script", new Dictionary<string, string> { {"type", "text/javascript"} } },
            { "stylesheet", new Dictionary<string, string> { {"type", "text/css"}, {"rel", "stylesheet"} } }
        };
        private static readonly Dictionary<string, TagRenderMode> _fileTagRenderModes = new Dictionary<string, TagRenderMode> {
            { "script", TagRenderMode.Normal },
            { "link", TagRenderMode.SelfClosing }
        };
        private static readonly Dictionary<string, string> _resourceTypeDirectories = new Dictionary<string, string> {
            {"script", "scripts/"},
            {"stylesheet", "styles/"}
        };

        private string _basePath;
        private readonly Dictionary<RequireSettings, string> _urlResolveCache = new Dictionary<RequireSettings, string>();

        public ResourceDefinition(ResourceManifest manifest, string type, string name)
        {
            Manifest = manifest;
            Type = type;
            Name = name;
            TagBuilder = new TagBuilder(_resourceTypeTagNames.ContainsKey(type) ? _resourceTypeTagNames[type] : "meta");
            TagRenderMode = _fileTagRenderModes.ContainsKey(TagBuilder.TagName) ? _fileTagRenderModes[TagBuilder.TagName] : TagRenderMode.Normal;
            Dictionary<string, string> attributes;
            if (_resourceAttributes.TryGetValue(type, out attributes))
            {
                foreach (var pair in attributes)
                {
                    TagBuilder.Attributes[pair.Key] = pair.Value;
                }
            }
            FilePathAttributeName = _filePathAttributes.ContainsKey(TagBuilder.TagName) ? _filePathAttributes[TagBuilder.TagName] : null;
        }

        internal static string GetBasePathFromViewPath(string resourceType, string viewPath)
        {
            if (String.IsNullOrEmpty(viewPath))
            {
                return null;
            }
            string basePath = null;
            var viewsPartIndex = viewPath.IndexOf("/Views", StringComparison.OrdinalIgnoreCase);
            if (viewsPartIndex >= 0)
            {
                basePath = viewPath.Substring(0, viewsPartIndex + 1) + GetResourcePath(resourceType);
            }
            return basePath;
        }

        internal static string GetResourcePath(string resourceType)
        {
            string path;
            _resourceTypeDirectories.TryGetValue(resourceType, out path);
            return path ?? "";
        }

        private static string Coalesce(params string[] strings)
        {
            foreach (var str in strings)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    return str;
                }
            }
            return null;
        }

        public IResourceManifest Manifest { get; private set; }
        public string TagName
        {
            get { return TagBuilder.TagName; }
        }
        public TagRenderMode TagRenderMode { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Version { get; private set; }
        public string BasePath
        {
            get
            {
                if (!String.IsNullOrEmpty(_basePath))
                {
                    return _basePath;
                }
                var basePath = Manifest.BasePath;
                if (!String.IsNullOrEmpty(basePath))
                {
                    basePath += GetResourcePath(Type);
                }
                return basePath ?? "";
            }
        }
        public string Url { get; private set; }
        public string UrlDebug { get; private set; }
        public string UrlCdn { get; private set; }
        public string UrlCdnDebug { get; private set; }
        public string[] Cultures { get; private set; }
        public bool CdnSupportsSsl { get; private set; }
        public IEnumerable<string> Dependencies { get; private set; }
        public string FilePathAttributeName { get; private set; }
        public TagBuilder TagBuilder { get; private set; }

        public ResourceDefinition AddAttribute(string name, string value)
        {
            TagBuilder.MergeAttribute(name, value);
            return this;
        }

        public ResourceDefinition SetAttribute(string name, string value)
        {
            TagBuilder.MergeAttribute(name, value, true);
            return this;
        }

        public ResourceDefinition SetBasePath(string virtualPath)
        {
            _basePath = virtualPath;
            return this;
        }

        public ResourceDefinition SetUrl(string url)
        {
            return SetUrl(url, null);
        }

        public ResourceDefinition SetUrl(string url, string urlDebug)
        {
            if (String.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            Url = url;
            if (urlDebug != null)
            {
                UrlDebug = urlDebug;
            }
            return this;
        }

        public ResourceDefinition SetCdn(string cdnUrl)
        {
            return SetCdn(cdnUrl, null, null);
        }

        public ResourceDefinition SetCdn(string cdnUrl, string cdnUrlDebug)
        {
            return SetCdn(cdnUrl, cdnUrlDebug, null);
        }

        public ResourceDefinition SetCdn(string cdnUrl, string cdnUrlDebug, bool? cdnSupportsSsl)
        {
            if (String.IsNullOrEmpty(cdnUrl))
            {
                throw new ArgumentNullException("cdnUrl");
            }
            UrlCdn = cdnUrl;
            if (cdnUrlDebug != null)
            {
                UrlCdnDebug = cdnUrlDebug;
            }
            if (cdnSupportsSsl.HasValue)
            {
                CdnSupportsSsl = cdnSupportsSsl.Value;
            }
            return this;
        }

        /// <summary>
        /// Sets the version of the resource.
        /// </summary>
        /// <param name="version">The version to set, in the form of <code>major.minor[.build[.revision]]</code></param>
        public ResourceDefinition SetVersion(string version)
        {
            Version = version;
            return this;
        }

        public ResourceDefinition SetCultures(params string[] cultures)
        {
            Cultures = cultures;
            return this;
        }

        public ResourceDefinition SetDependencies(params string[] dependencies)
        {
            Dependencies = dependencies;
            return this;
        }

        public string ResolveUrl(RequireSettings settings, string applicationPath)
        {
            string url;
            if (_urlResolveCache.TryGetValue(settings, out url))
            {
                return url;
            }
            // Url priority:
            if (settings.DebugMode)
            {
                url = settings.CdnMode
                    ? Coalesce(UrlCdnDebug, UrlDebug, UrlCdn, Url)
                    : Coalesce(UrlDebug, Url, UrlCdnDebug, UrlCdn);
            }
            else
            {
                url = settings.CdnMode
                    ? Coalesce(UrlCdn, Url, UrlCdnDebug, UrlDebug)
                    : Coalesce(Url, UrlDebug, UrlCdn, UrlCdnDebug);
            }
            if (String.IsNullOrEmpty(url))
            {
                return null;
            }
            if (!String.IsNullOrEmpty(settings.Culture))
            {
                string nearestCulture = FindNearestCulture(settings.Culture);
                if (!String.IsNullOrEmpty(nearestCulture))
                {
                    url = Path.ChangeExtension(url, nearestCulture + Path.GetExtension(url));
                }
            }
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !VirtualPathUtility.IsAbsolute(url) && !VirtualPathUtility.IsAppRelative(url) && !String.IsNullOrEmpty(BasePath))
            {
                // relative urls are relative to the base path of the module that defined the manifest
                url = VirtualPathUtility.Combine(BasePath, url);
            }
            if (VirtualPathUtility.IsAppRelative(url))
            {
                url = applicationPath != null
                    ? VirtualPathUtility.ToAbsolute(url, applicationPath)
                    : VirtualPathUtility.ToAbsolute(url);
            }
            _urlResolveCache[settings] = url;
            return url;
        }

        public string FindNearestCulture(string culture)
        {
            // go for an exact match
            if (Cultures == null)
            {
                return null;
            }
            int selectedIndex = Array.IndexOf(Cultures, culture);
            if (selectedIndex != -1)
            {
                return Cultures[selectedIndex];
            }
            // try parent culture if any
            var cultureInfo = new CultureInfo(culture);
            if (cultureInfo.Parent.Name != culture)
            {
                var selectedCulture = FindNearestCulture(cultureInfo.Parent.Name);
                if (selectedCulture != null)
                {
                    return selectedCulture;
                }
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var that = (ResourceDefinition)obj;
            return string.Equals(that.Name, Name, StringComparison.Ordinal) &&
                string.Equals(that.Type, Type, StringComparison.Ordinal) &&
                string.Equals(that.Version, Version, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode() ^ (Type ?? "").GetHashCode();
        }

    }
}

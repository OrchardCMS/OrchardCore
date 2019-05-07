using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.ResourceManagement
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
            { "link", TagRenderMode.SelfClosing },
            { "stylesheet", TagRenderMode.SelfClosing }
        };

        private string _basePath;

        public ResourceDefinition(ResourceManifest manifest, string type, string name)
        {
            Manifest = manifest;
            Type = type;
            Name = name;

            TagName = _resourceTypeTagNames.ContainsKey(Type) ? _resourceTypeTagNames[Type] : "meta";
            FilePathAttributeName = _filePathAttributes.ContainsKey(TagName) ? _filePathAttributes[TagName] : null;
            TagRenderMode = _fileTagRenderModes.ContainsKey(TagName) ? _fileTagRenderModes[TagName] : TagRenderMode.Normal;
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

        public ResourceManifest Manifest { get; private set; }

        public string TagName { get; private set; }
        public TagRenderMode TagRenderMode { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Version { get; private set; }
        public bool? AppendVersion { get; private set; }
        public string Url { get; private set; }
        public string UrlDebug { get; private set; }
        public string UrlCdn { get; private set; }
        public string UrlCdnDebug { get; private set; }
        public string CdnDebugIntegrity { get; private set; }
        public string CdnIntegrity { get; private set; }
        public string[] Cultures { get; private set; }
        public bool CdnSupportsSsl { get; private set; }
        public List<string> Dependencies { get; private set; }
        public string FilePathAttributeName { get; private set; }
        public AttributeDictionary Attributes { get; private set; }

        public ResourceDefinition SetAttribute(string name, string value)
        {
            if (Attributes == null)
            {
                Attributes = new AttributeDictionary();
            }

            Attributes[name] = value;
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

        public ResourceDefinition SetCdnIntegrity(string cdnIntegrity)
        {
            return SetCdnIntegrity(cdnIntegrity, null);
        }

        public ResourceDefinition SetCdnIntegrity(string cdnIntegrity, string cdnDebugIntegrity)
        {
            if (String.IsNullOrEmpty(cdnIntegrity))
            {
                throw new ArgumentNullException("cdnUrl");
            }
            CdnIntegrity = cdnIntegrity;
            if (cdnDebugIntegrity != null)
            {
                CdnDebugIntegrity = cdnDebugIntegrity;
            }
            return this;
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

        public ResourceDefinition SetAppendVersion(bool? appendVersion)
        {
            AppendVersion = appendVersion;
            return this;
        }

        public ResourceDefinition SetCultures(params string[] cultures)
        {
            Cultures = cultures;
            return this;
        }

        public ResourceDefinition SetDependencies(params string[] dependencies)
        {
            if (Dependencies == null)
            {
                Dependencies = new List<string>();
            }

            Dependencies.AddRange(dependencies);

            return this;
        }

        public TagBuilder GetTagBuilder(RequireSettings settings,
            string applicationPath,
            IFileVersionProvider fileVersionProvider)
        {
            string url;
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
                url = null;
            }
            if (!String.IsNullOrEmpty(settings.Culture))
            {
                string nearestCulture = FindNearestCulture(settings.Culture);
                if (!String.IsNullOrEmpty(nearestCulture))
                {
                    url = Path.ChangeExtension(url, nearestCulture + Path.GetExtension(url));
                }
            }

            if (url.StartsWith("~/", StringComparison.Ordinal))
            {
                if (!String.IsNullOrEmpty(_basePath))
                {
                    url = _basePath + url.Substring(1);
                }
                else
                {
                    url = applicationPath + url.Substring(1);
                }
            }

            // If settings has value, it can override resource definition, otherwise use resource definition
            if (url != null && ((settings.AppendVersion.HasValue && settings.AppendVersion == true) ||
                (!settings.AppendVersion.HasValue && AppendVersion == true)))
            {
                url = fileVersionProvider.AddFileVersionToPath(applicationPath, url);
            }

            // Don't prefix cdn if the path is absolute, or is in debug mode.
            if (!settings.DebugMode
                && !String.IsNullOrEmpty(settings.CdnBaseUrl)
                && !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                url = settings.CdnBaseUrl + url;
            }

            var tagBuilder = new TagBuilder(TagName)
            {
                TagRenderMode = TagRenderMode
            };

            if (!String.IsNullOrEmpty(CdnIntegrity) && url != null && url == UrlCdn)
            {
                tagBuilder.Attributes["integrity"] = CdnIntegrity;
                tagBuilder.Attributes["crossorigin"] = "anonymous";
            }
            else if (!String.IsNullOrEmpty(CdnDebugIntegrity) && url != null && url == UrlCdnDebug)
            {
                tagBuilder.Attributes["integrity"] = CdnDebugIntegrity;
                tagBuilder.Attributes["crossorigin"] = "anonymous";
            }

            if (_resourceAttributes.ContainsKey(Type))
            {
                tagBuilder.MergeAttributes(_resourceAttributes[Type]);
            }

            if (Attributes != null)
            {
                tagBuilder.MergeAttributes(Attributes);
            }

            if (settings.HasAttributes)
            {
                tagBuilder.MergeAttributes(settings.Attributes);
            }

            if (!String.IsNullOrEmpty(FilePathAttributeName))
            {
                if (!String.IsNullOrEmpty(url))
                {
                    tagBuilder.MergeAttribute(FilePathAttributeName, url, true);
                }
            }

            return tagBuilder;
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

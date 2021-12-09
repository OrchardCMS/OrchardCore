using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.ResourceManagement
{
    public class ResourceDefinition
    {
        private string _basePath;

        public ResourceDefinition(ResourceManifest manifest, string type, string name)
        {
            Manifest = manifest;
            Type = type;
            Name = name;
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
        public AttributeDictionary Attributes { get; private set; }
        public string InnerContent { get; private set; }
        public ResourcePosition Position { get; private set; }

        public ResourceDefinition SetAttribute(string name, string value)
        {
            if (Attributes == null)
            {
                Attributes = new AttributeDictionary();
            }

            Attributes[name] = value;
            return this;
        }

        public ResourceDefinition SetBasePath(string basePath)
        {
            _basePath = basePath;
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
                ThrowArgumentNullException(nameof(url));
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
                ThrowArgumentNullException(nameof(cdnIntegrity));
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
                ThrowArgumentNullException(nameof(cdnUrl));
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

        /// <summary>
        /// Should a file version be appended to the resource.
        /// </summary>
        /// <param name="appendVersion"></param>
        public ResourceDefinition ShouldAppendVersion(bool? appendVersion)
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

        public ResourceDefinition SetInnerContent(string innerContent)
        {
            InnerContent = innerContent;

            return this;
        }
        /// <summary>
        /// Position a resource first, last or by dependency.
        /// </summary>
        /// <param name="position"></param>
        public ResourceDefinition SetPosition(ResourcePosition position)
        {
            Position = position;

            return this;
        }

        public TagBuilder GetTagBuilder(RequireSettings settings,
            string applicationPath,
            IFileVersionProvider fileVersionProvider)
        {
            string url, filePathAttributeName = null;
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
                var nearestCulture = FindNearestCulture(settings.Culture);
                if (!String.IsNullOrEmpty(nearestCulture))
                {
                    url = Path.ChangeExtension(url, nearestCulture + Path.GetExtension(url));
                }
            }

            if (url != null && url.StartsWith("~/", StringComparison.Ordinal))
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

            // Don't prefix cdn if the path includes a protocol, i.e. is an external url, or is in debug mode.
            if (url != null && !settings.DebugMode && !String.IsNullOrEmpty(settings.CdnBaseUrl) &&
                // Don't evaluate with Uri.TryCreate as it produces incorrect results on Linux.
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("//", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                url = settings.CdnBaseUrl + url;
            }

            TagBuilder tagBuilder;
            switch (Type)
            {
                case "script":
                    tagBuilder = new TagBuilder("script");
                    if (settings.Attributes.Count > 0)
                    {
                        foreach (var kv in settings.Attributes)
                        {
                            tagBuilder.Attributes.Add(kv);
                        }
                    }
                    filePathAttributeName = "src";
                    break;
                case "stylesheet":
                    if (url == null && InnerContent != null)
                    {
                        // Inline style declaration
                        tagBuilder = new TagBuilder("style")
                        {
                            Attributes = {
                                { "type", "text/css" }
                            }
                        };
                    }
                    else
                    {
                        // Stylesheet resource
                        tagBuilder = new TagBuilder("link")
                        {
                            TagRenderMode = TagRenderMode.SelfClosing,
                            Attributes = {
                                { "type", "text/css" },
                                { "rel", "stylesheet" }
                            }
                        };
                        filePathAttributeName = "href";
                    }
                    break;
                case "link":
                    tagBuilder = new TagBuilder("link") { TagRenderMode = TagRenderMode.SelfClosing };
                    filePathAttributeName = "href";
                    break;
                default:
                    tagBuilder = new TagBuilder("meta") { TagRenderMode = TagRenderMode.SelfClosing };
                    break;
            }

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

            if (Attributes != null)
            {
                tagBuilder.MergeAttributes(Attributes);
            }

            if (settings.HasAttributes)
            {
                tagBuilder.MergeAttributes(settings.Attributes);
            }

            if (!String.IsNullOrEmpty(url) && filePathAttributeName != null)
            {
                tagBuilder.MergeAttribute(filePathAttributeName, url, true);
            }
            else if (!String.IsNullOrEmpty(InnerContent))
            {
                tagBuilder.InnerHtml.AppendHtml(InnerContent);
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
            var selectedIndex = Array.IndexOf(Cultures, culture);
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
            return String.Equals(that.Name, Name) &&
                String.Equals(that.Type, Type) &&
                String.Equals(that.Version, Version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}

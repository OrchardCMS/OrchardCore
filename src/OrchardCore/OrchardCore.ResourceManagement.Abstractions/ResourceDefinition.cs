using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.ResourceManagement;

public class ResourceDefinition
{
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
            if (!string.IsNullOrEmpty(str))
            {
                return str;
            }
        }
        return null;
    }

    public ResourceManifest Manifest { get; private set; }

    public string BasePath { get; private set; }
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
        Attributes ??= [];

        Attributes[name] = value;
        return this;
    }

    public ResourceDefinition SetBasePath(string basePath)
    {
        BasePath = basePath;
        return this;
    }

    public ResourceDefinition SetUrl(string url)
    {
        return SetUrl(url, null);
    }

    public ResourceDefinition SetUrl(string url, string urlDebug)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

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
        ArgumentException.ThrowIfNullOrEmpty(cdnIntegrity);

        CdnIntegrity = cdnIntegrity;
        if (cdnDebugIntegrity != null)
        {
            CdnDebugIntegrity = cdnDebugIntegrity;
        }
        return this;
    }

    public ResourceDefinition SetCdn(string cdnUrl, string cdnUrlDebug, bool? cdnSupportsSsl)
    {
        ArgumentException.ThrowIfNullOrEmpty(cdnUrl);

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
    /// <param name="version">The version to set, in the form of. <code>major.minor[.build[.revision]]</code></param>
    public ResourceDefinition SetVersion(string version)
    {
        if (!System.Version.TryParse(version, out _))
        {
            throw new FormatException("The resource version should be in the form of major.minor[.build[.revision]].");
        }

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
        Dependencies ??= [];

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

        // Url priority.
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

        if (string.IsNullOrEmpty(url))
        {
            url = null;
        }
        if (!string.IsNullOrEmpty(settings.Culture))
        {
            var nearestCulture = FindNearestCulture(settings.Culture);
            if (!string.IsNullOrEmpty(nearestCulture))
            {
                url = Path.ChangeExtension(url, nearestCulture + Path.GetExtension(url));
            }
        }

        if (url != null && url.StartsWith("~/", StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(BasePath))
            {
                url = string.Concat(BasePath, url.AsSpan(1));
            }
            else
            {
                url = string.Concat(applicationPath, url.AsSpan(1));
            }
        }

        // If settings has value, it can override resource definition, otherwise use resource definition.
        if (url != null && ((settings.AppendVersion.HasValue && settings.AppendVersion == true) ||
            (!settings.AppendVersion.HasValue && AppendVersion == true)))
        {
            url = fileVersionProvider.AddFileVersionToPath(applicationPath, url);
        }

        // Don't prefix cdn if the path includes a protocol, i.e. is an external url, or is in debug mode.
        if (url != null && !settings.DebugMode && !string.IsNullOrEmpty(settings.CdnBaseUrl) &&

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
                    // Inline style declaration.
                    tagBuilder = new TagBuilder("style")
                    {
                        Attributes = {
                            { "type", "text/css" }
                        }
                    };
                }
                else
                {
                    // Stylesheet resource.
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

        if (!string.IsNullOrEmpty(CdnIntegrity) && url != null && url == UrlCdn)
        {
            tagBuilder.Attributes["integrity"] = CdnIntegrity;
            tagBuilder.Attributes["crossorigin"] = "anonymous";
        }
        else if (!string.IsNullOrEmpty(CdnDebugIntegrity) && url != null && url == UrlCdnDebug)
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

        if (!string.IsNullOrEmpty(url) && filePathAttributeName != null)
        {
            tagBuilder.MergeAttribute(filePathAttributeName, url, true);
        }
        else if (!string.IsNullOrEmpty(InnerContent))
        {
            tagBuilder.InnerHtml.AppendHtml(InnerContent);
        }

        return tagBuilder;
    }

    public string FindNearestCulture(string culture)
    {
        // Go for an exact match.
        if (Cultures == null)
        {
            return null;
        }
        var selectedIndex = Array.IndexOf(Cultures, culture);
        if (selectedIndex != -1)
        {
            return Cultures[selectedIndex];
        }
        // Try parent culture if any.
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
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
        return HashCode.Combine(Name, Type);
    }
}

using System.Globalization;
using System.Net.Mime;
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

    /// <summary>
    /// Gets the parsed <see cref="Version"/>, or <see langword="null"/> if the resource has no version.
    /// </summary>
    public Version ParsedVersion { get; private set; }

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
        if (!System.Version.TryParse(version, out var parsedVersion))
        {
            throw new FormatException("The resource version should be in the form of major.minor[.build[.revision]].");
        }

        Version = version;
        ParsedVersion = parsedVersion;

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
        var url = GetUrl(settings, applicationPath, fileVersionProvider);

        TagBuilder tagBuilder;
        string filePathAttributeName = null;

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
                            { "type", MediaTypeNames.Text.Css },
                        },
                    };
                }
                else
                {
                    // Stylesheet resource.
                    tagBuilder = new TagBuilder("link")
                    {
                        TagRenderMode = TagRenderMode.SelfClosing,
                        Attributes = {
                            { "type", MediaTypeNames.Text.Css },
                            { "rel", "stylesheet" },
                        },
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

    /// <summary>
    /// Writes the tag of the resource, as <see cref="GetTagBuilder"/> would render it with a null encoder,
    /// without building the tag and its attributes dictionary for each rendered resource.
    /// </summary>
    public void WriteTo(TextWriter writer, RequireSettings settings, string applicationPath, IFileVersionProvider fileVersionProvider)
    {
        var url = GetUrl(settings, applicationPath, fileVersionProvider);

        var attributes = new TagAttributes();

        string tagName, filePathAttributeName = null;
        var selfClosing = false;

        switch (Type)
        {
            case "script":
                tagName = "script";
                if (settings.HasAttributes)
                {
                    foreach (var kv in settings.Attributes)
                    {
                        attributes.Set(kv.Key, kv.Value, replaceExisting: false);
                    }
                }
                filePathAttributeName = "src";
                break;
            case "stylesheet":
                if (url == null && InnerContent != null)
                {
                    // Inline style declaration.
                    tagName = "style";
                    attributes.Set("type", MediaTypeNames.Text.Css, replaceExisting: true);
                }
                else
                {
                    // Stylesheet resource.
                    tagName = "link";
                    selfClosing = true;
                    attributes.Set("type", MediaTypeNames.Text.Css, replaceExisting: true);
                    attributes.Set("rel", "stylesheet", replaceExisting: true);
                    filePathAttributeName = "href";
                }
                break;
            case "link":
                tagName = "link";
                selfClosing = true;
                filePathAttributeName = "href";
                break;
            default:
                tagName = "meta";
                selfClosing = true;
                break;
        }

        if (!string.IsNullOrEmpty(CdnIntegrity) && url != null && url == UrlCdn)
        {
            attributes.Set("integrity", CdnIntegrity, replaceExisting: true);
            attributes.Set("crossorigin", "anonymous", replaceExisting: true);
        }
        else if (!string.IsNullOrEmpty(CdnDebugIntegrity) && url != null && url == UrlCdnDebug)
        {
            attributes.Set("integrity", CdnDebugIntegrity, replaceExisting: true);
            attributes.Set("crossorigin", "anonymous", replaceExisting: true);
        }

        if (Attributes != null)
        {
            foreach (var kv in Attributes)
            {
                attributes.Set(kv.Key, kv.Value, replaceExisting: false);
            }
        }

        if (settings.HasAttributes)
        {
            foreach (var kv in settings.Attributes)
            {
                attributes.Set(kv.Key, kv.Value, replaceExisting: false);
            }
        }

        string innerContent = null;

        if (!string.IsNullOrEmpty(url) && filePathAttributeName != null)
        {
            attributes.Set(filePathAttributeName, url, replaceExisting: true);
        }
        else if (!string.IsNullOrEmpty(InnerContent))
        {
            innerContent = InnerContent;
        }

        writer.Write('<');
        writer.Write(tagName);
        attributes.WriteTo(writer);

        if (selfClosing)
        {
            writer.Write(" />");
            return;
        }

        writer.Write('>');

        if (innerContent != null)
        {
            writer.Write(innerContent);
        }

        writer.Write("</");
        writer.Write(tagName);
        writer.Write('>');
    }

    private string GetUrl(RequireSettings settings, string applicationPath, IFileVersionProvider fileVersionProvider)
    {
        string url;

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

        return url;
    }

    /// <summary>
    /// The attributes of a tag, kept sorted by name ignoring the case, as the attributes dictionary of a tag builder.
    /// </summary>
    private struct TagAttributes
    {
        private KeyValuePair<string, string>[] _items;
        private int _count;

        public void Set(string name, string value, bool replaceExisting)
        {
            _items ??= new KeyValuePair<string, string>[8];

            var index = 0;
            while (index < _count)
            {
                var comparison = StringComparer.OrdinalIgnoreCase.Compare(_items[index].Key, name);

                if (comparison == 0)
                {
                    if (replaceExisting)
                    {
                        _items[index] = new KeyValuePair<string, string>(name, value);
                    }

                    return;
                }

                if (comparison > 0)
                {
                    break;
                }

                index++;
            }

            if (_count == _items.Length)
            {
                Array.Resize(ref _items, _count * 2);
            }

            if (index < _count)
            {
                Array.Copy(_items, index, _items, index + 1, _count - index);
            }

            _items[index] = new KeyValuePair<string, string>(name, value);
            _count++;
        }

        public readonly void WriteTo(TextWriter writer)
        {
            for (var i = 0; i < _count; i++)
            {
                var (name, value) = _items[i];

                if (string.Equals(name, "id", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(value))
                {
                    continue;
                }

                writer.Write(' ');
                writer.Write(name);
                writer.Write("=\"");
                writer.Write(value);
                writer.Write('"');
            }
        }
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

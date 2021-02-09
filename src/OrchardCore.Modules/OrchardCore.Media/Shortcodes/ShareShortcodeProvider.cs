using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.ResourceManagement;
using Shortcodes;

namespace OrchardCore.Media.Shortcodes
{
    public class ShortcodeProperty
    {
        public string Name
        {
            get; set;
        }

        public string Mapping
        {
            get; set;
        }

        private ShortcodeProperty(string name) : this(name, name)
        {
        }

        private ShortcodeProperty(string name, string mapping)
        {
            Name = name;
            Mapping = mapping;
        }

        public static ShortcodeProperty Url
        {
            get
            {
                return new ShortcodeProperty("url", "href");
            }
        }
        public static ShortcodeProperty Save
        {
            get
            {
                return new ShortcodeProperty("save", "download");
            }
        }
        public static ShortcodeProperty Tooltip
        {
            get
            {
                return new ShortcodeProperty("tooltip", "title");
            }
        }
        public static ShortcodeProperty Class
        {
            get
            {
                return new ShortcodeProperty("class");
            }
        }
    }

    public class ShareShortCodeProvider : IShortcodeProvider
    {

        private const string FileExtensionSeperator = ".";
        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);
        private static readonly ValueTask<string> ShareShortcode = new ValueTask<string>("[share]");
        private static readonly HashSet<string> Shortcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "share",
            "doc",
            "download"
        };

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;

        public ShareShortCodeProvider(
            IMediaFileStore mediaFileStore,
            IHtmlSanitizerService htmlSanitizerService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ResourceManagementOptions> options)
        {
            _mediaFileStore = mediaFileStore;
            _htmlSanitizerService = htmlSanitizerService;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        #region https://stackoverflow.com/a/40361205/549306
        readonly static Uri SomeBaseUri = new Uri("http://canbeanything");

        static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                uri = new Uri(SomeBaseUri, url);

            return Path.GetFileName(uri.LocalPath);
        }

        #endregion

        public ValueTask<string> EvaluateAsync(string identifier, Arguments arguments, string content, Context context)
        {
            // we can't process what we don't understand, and we are discarding or filtering out anything we don't understand to keep things safe
            if (!Shortcodes.Contains(identifier))
            {
                return Null;
            }

            // critical that at least the content or the href/url/source is set
            if (String.IsNullOrWhiteSpace(content) && !IsSet(arguments, ShortcodeProperty.Url))
            {
                return ShareShortcode;
            }

            string url;
            string href;
            // sort out and infer as necessary the description (content) and resource location (url and href)
            if (IsSet(arguments, ShortcodeProperty.Url))
            {
                url = arguments.NamedOrDefault(ShortcodeProperty.Url.Name);
                url = UrlFixer(url, _httpContextAccessor.HttpContext.Request.PathBase, _options.CdnBaseUrl, _mediaFileStore);
                href = FormatAsHtmlElementAttribute(ShortcodeProperty.Url, url);
                if (String.IsNullOrWhiteSpace(content))
                {
                    content = GetFileNameFromUrl(url).Split(FileExtensionSeperator)[0];
                }
            }
            else
            {
                url = UrlFixer(content, _httpContextAccessor.HttpContext.Request.PathBase, _options.CdnBaseUrl, _mediaFileStore);
                href = FormatAsHtmlElementAttribute(ShortcodeProperty.Url, url);
                // because both the content and the href can't be empty, we presume the content was the href in this branch, and replace a shortened, friendlier name for the resulting content
                content = GetFileNameFromUrl(content).Split(FileExtensionSeperator)[0];
            }

            var saveAs = IsSet(arguments, ShortcodeProperty.Save) ? arguments.NamedOrDefault(ShortcodeProperty.Save.Name) : GetFileNameFromUrl(url);
            saveAs = FormatAsHtmlElementAttribute(ShortcodeProperty.Save, saveAs);

            // process contextually benign adornments
            var classList = String.Empty;
            var titleText = String.Empty;
            if (arguments.Any())
            {

                classList = FormatArguement(arguments, ShortcodeProperty.Class);
                titleText = FormatArguement(arguments, ShortcodeProperty.Tooltip);
                //var queryStringParams = new Dictionary<string, string>();
                //content = QueryHelpers.AddQueryString(content, queryStringParams);
            }

            content = String.Format("<a {0} {1} {2} {3}>{4}</a>", new string[] { titleText, classList, href, saveAs, content });
            content = _htmlSanitizerService.Sanitize(content);

            return new ValueTask<string>(content);
        }

        private static string FormatAsHtmlElementAttribute(ShortcodeProperty property, string value)
        {
            return !String.IsNullOrEmpty(value) ? String.Format("{0}=\"{1}\"", new string[] { property.Mapping, value }) : String.Empty;
        }

        private static bool IsSet(Arguments arguments, ShortcodeProperty property)
        {
            return !String.IsNullOrWhiteSpace(arguments.NamedOrDefault(property.Name));
        }

        private static string FormatArguement(Arguments arguments, ShortcodeProperty property)
        {
            return FormatAsHtmlElementAttribute(property, arguments.Named(property.Name));
        }

        private static string UrlFixer(string url, PathString requestPath, string cdnBase, IMediaFileStore media)
        {
            if (IsSchemeless(url) || IsHttpOrHttps(url))
            {
                return url;
            }

            if (IsVirtualPath(url))
            {
                url = requestPath.Add(url[1..]).Value;
                // using a cdn?
                if (!String.IsNullOrEmpty(cdnBase))
                {
                    url = cdnBase + url;
                }
            }
            else
            {
                url = media.MapPathToPublicUrl(url);
            }
            return url;
        }

        // Serve static files from virtual path (multi-tenancy)
        private static bool IsVirtualPath(string url) => url.StartsWith("~/", StringComparison.Ordinal);
        private static bool IsHttpOrHttps(string url) => url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        private static bool IsSchemeless(string url) => url.StartsWith("//", StringComparison.Ordinal);
    }
}

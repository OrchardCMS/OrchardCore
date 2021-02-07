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
            get;set;
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
            string saveAs;

            if (!IsSet(arguments, ShortcodeProperty.Url))
            {
                url = content;
                href = FormatAsHtmlElementAttribute(ShortcodeProperty.Url, content);
                // because both the content and the href can't be empty, we presume the content was the href in this branch, and replace a shortened, friendlier name for the resulting content
                content = GetFileNameFromUrl(content).Split(FileExtensionSeperator)[0];
            }
            else
            {
                url = arguments.NamedOrDefault(ShortcodeProperty.Url.Name);
                href = GetArguement(arguments, ShortcodeProperty.Url);
                if (String.IsNullOrWhiteSpace(content))
                {
                    content = GetFileNameFromUrl(url).Split(FileExtensionSeperator)[0];
                }
            }

            if (!IsSet(arguments, ShortcodeProperty.Save))
            {
                saveAs = GetFileNameFromUrl(url);
            }
            else
            {
                saveAs = arguments.NamedOrDefault(ShortcodeProperty.Save.Name);
            }
            saveAs = FormatAsHtmlElementAttribute(ShortcodeProperty.Save, saveAs);

            //if (!content.StartsWith("//", StringComparison.Ordinal) && !content.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            //{
            //    // Serve static files from virtual path.
            //    if (content.StartsWith("~/", StringComparison.Ordinal))
            //    {
            //        content = _httpContextAccessor.HttpContext.Request.PathBase.Add(content[1..]).Value;
            //        if (!String.IsNullOrEmpty(_options.CdnBaseUrl))
            //        {
            //            content = _options.CdnBaseUrl + content;
            //        }
            //    }
            //    else
            //    {
            //        content = _mediaFileStore.MapPathToPublicUrl(content);
            //    }
            //}

            var classList = String.Empty;
            var titleText = String.Empty;
            if (arguments.Any())
            {

                classList = GetArguement(arguments, ShortcodeProperty.Class);
                titleText = GetArguement(arguments, ShortcodeProperty.Tooltip);
                //var queryStringParams = new Dictionary<string, string>();
                //content = QueryHelpers.AddQueryString(content, queryStringParams);
            }

            content = String.Format("<a {0} {1} {2} {3}>{4}</a>", new string[] { titleText, classList, href, saveAs, content });
            content = _htmlSanitizerService.Sanitize(content);

            return new ValueTask<string>(content);
        }

        private static string FormatAsHtmlElementAttribute(ShortcodeProperty property, string value) => String.Format("{0}=\"{1}\"", new string[] {property.Mapping, value});

        private static bool IsSet(Arguments arguments, ShortcodeProperty key)
        {
            return !String.IsNullOrWhiteSpace(arguments.NamedOrDefault(key.Name));
        }

        private static string GetArguement(Arguments arguments, ShortcodeProperty property)
        {
            var value = arguments.Named(property.Name);
            if (!String.IsNullOrEmpty(value))
            {
                value = FormatAsHtmlElementAttribute(property, value);
            }

            return value;
        }
    }
}

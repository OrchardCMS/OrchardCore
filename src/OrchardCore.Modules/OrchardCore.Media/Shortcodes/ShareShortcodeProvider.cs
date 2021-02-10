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

    /// <summary>
    /// Provides a 'short-code' for creating an anchor tag link to share items for download from the cdn or tenant library.
    /// It operates much like the image short-code, but is distinct and does not explicitly share common code at the time
    /// of writing (TODO: but it could be and should be refactored to do so).
    ///
    /// The only required value is a "valid" url.  There is no restriction on local-only resources, but there easily could be.
    /// Using the url, either from the content of the short-code, or as the value to the "url" arguement, a functional link with
    /// a somewhat descriptive text can be inferred by the given url.  If the "url" arguement is supplied, but the content is not,
    /// the content is assumed to be the filename part of the given url, dropping any file extension.   When clicked, the content
    /// should not be displayed in the browser (for anything maintstream that is currently supported), but rather the user should
    /// prompted to save it using a suggested but not mandatory filename either specified using the "save" arguement, or inferred
    /// from the content url.  Please note that the short-code argument parser presumes spaces are delimiters, and so content urls
    /// that include spaces must be quoted; however, this provider will render an encoded url, replacing characters such as spaces
    /// with their url-hexidecimal format (eg space would be %20).
    ///
    /// Additional optional arguements can be specified for styling and accessibility purposes.  These include:
    /// - tooltip: to create a 'title' attribute on the anchor, and again the shortcode arguement parser presumes a space to be a
    /// delimiter for the next arguement pair, and so quotes are required for multi-word titles
    /// - class: to specify the css classes to attach to the element
    /// </summary>
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
        private static readonly Uri SomeBaseUri = new Uri("http://canbeanything");

        private static string GetFileNameFromUrl(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                uri = new Uri(SomeBaseUri, url);
            }

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
                url = UrlFixer(arguments.NamedOrDefault(ShortcodeProperty.Url.Name));
                if (String.IsNullOrWhiteSpace(content))
                {
                    content = GetFileNameFromUrl(url).Split(FileExtensionSeperator)[0];
                }
            }
            else
            {
                url = UrlFixer(content);
                // because both the content and the href can't be empty, we presume the content was the href in this branch, and replace a shortened, friendlier name for the resulting content
                content = GetFileNameFromUrl(content).Split(FileExtensionSeperator)[0];
            }

            // urlencode to fix space in path rendering
            href = FormatAsHtmlElementAttribute(ShortcodeProperty.Url, Uri.EscapeUriString(url));
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

        private static string FormatAsHtmlElementAttribute(ShortcodeProperty property, string value) => !String.IsNullOrEmpty(value) ? String.Format("{0}=\"{1}\"", new string[] { property.Mapping, value }) : String.Empty;

        private static bool IsSet(Arguments arguments, ShortcodeProperty property) => !String.IsNullOrWhiteSpace(arguments.NamedOrDefault(property.Name));

        private static string FormatArguement(Arguments arguments, ShortcodeProperty property) => FormatAsHtmlElementAttribute(property, arguments.Named(property.Name));

        private string UrlFixer(string url)
        {

            if (IsSchemeless(url) || IsHttpOrHttps(url))
            {
                return url;
            }

            if (IsVirtualPath(url))
            {
                var requestPath = _httpContextAccessor.HttpContext.Request.PathBase;
                url = requestPath.Add(url[1..]).Value;
                // using a cdn?
                if (!String.IsNullOrEmpty(_options.CdnBaseUrl))
                {
                    url = _options.CdnBaseUrl + url;
                }
            }
            else
            {
                url = _mediaFileStore.MapPathToPublicUrl(url);
            }

 

            return url;
        }

        // Serve static files from virtual path (multi-tenancy)
        private static bool IsVirtualPath(string url) => url.StartsWith("~/", StringComparison.Ordinal);
        private static bool IsHttpOrHttps(string url) => url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        private static bool IsSchemeless(string url) => url.StartsWith("//", StringComparison.Ordinal);
    }
}

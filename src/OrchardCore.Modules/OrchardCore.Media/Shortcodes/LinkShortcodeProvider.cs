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
    public class LinkShortcodeProvider
        : IShortcodeProvider
    {

        private static readonly ValueTask<string> Null = new ValueTask<string>((string)null);
        private static readonly ValueTask<string> LinkShortcode = new ValueTask<string>("[link]");
        private static readonly HashSet<string> Shortcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "link",
        };

        private readonly IMediaFileStore _mediaFileStore;
        private readonly IHtmlSanitizerService _htmlSanitizerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ResourceManagementOptions _options;

        public LinkShortcodeProvider(
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
        private static readonly Uri SomeBaseUri = new Uri("http://example.com");

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
            if (String.IsNullOrWhiteSpace(content) && arguments.NamedOrDefault(LinkShortcodeProperties.Url.Name) is null)
            {
                return LinkShortcode;
            }

            string url;
            // sort out and infer as necessary the description (content) and resource location (url and href)
            if (arguments.NamedOrDefault(LinkShortcodeProperties.Url.Name) is null)
            {
                url = UrlFixer(content);
                content = url;
            }
            else
            {
                url = UrlFixer(arguments.NamedOrDefault(LinkShortcodeProperties.Url.Name));
                if (String.IsNullOrWhiteSpace(content))
                {
                    content = url;
                }
            }

            // urlencode to fix space in path rendering
            var href = FormatAsAttributePair(LinkShortcodeProperties.Url, Uri.EscapeUriString(url));

            // construct a browser-friendly 'save as' filename
            var saveAs = arguments.NamedOrDefault(LinkShortcodeProperties.Save.Name) is null ? GetFileNameFromUrl(url) : arguments.NamedOrDefault(LinkShortcodeProperties.Save.Name);
            saveAs = FormatAsAttributePair(LinkShortcodeProperties.Save, saveAs);

            // process contextually benign link adornments
            var classList = String.Empty;
            var titleText = String.Empty;
            if (arguments.Any())
            {

                classList = FormatArguement(arguments, LinkShortcodeProperties.Class);
                titleText = FormatArguement(arguments, LinkShortcodeProperties.Tooltip);
            }

            // render sanitized html
            content = $"<a {titleText} {classList} {href} {saveAs}>{content}</a>";
            content = _htmlSanitizerService.Sanitize(content);

            return new ValueTask<string>(content);
        }

        private static string FormatAsAttributePair(LinkShortcodeProperties property, string value) => String.IsNullOrEmpty(value) ? String.Empty: $"{property.Mapping}=\"{value}\"";

        private static string FormatArguement(Arguments arguments, LinkShortcodeProperties property) => FormatAsAttributePair(property, arguments.Named(property.Name));

        private string UrlFixer(string url)
        {

            if (IsSchemeless(url) || IsHttpOrHttps(url))
            {
                return url;
            }

            if (IsVirtualPath(url))
            {
                // add tenant path part
                var requestPath = _httpContextAccessor.HttpContext.Request.PathBase;
                url = requestPath.Add(url[1..]).Value;
                // using a cdn?
                if (IsUsingCdn())
                {
                    // add cdn part
                    url = _options.CdnBaseUrl + url;
                }
            }
            else
            {
                // add media store part
                url = _mediaFileStore.MapPathToPublicUrl(url);
            }

            return url;
        }

        private bool IsUsingCdn() => !String.IsNullOrEmpty(_options.CdnBaseUrl);

        // Serve static files from virtual path (multi-tenancy)
        private static bool IsVirtualPath(string url) => url.StartsWith("~/", StringComparison.Ordinal);
        private static bool IsHttpOrHttps(string url) => url.StartsWith("http", StringComparison.OrdinalIgnoreCase);
        private static bool IsSchemeless(string url) => url.StartsWith("//", StringComparison.Ordinal);
    }
}

using System.ComponentModel;

namespace OrchardCore.Html.Settings
{
    public class HtmlBodyPartSettings
    {
        /// <summary>
        /// Whether to sanitize the html input.
        /// When true liquid templating is disabled.
        /// </summary>
        [DefaultValue(true)]
        public bool SanitizeHtml { get; set; } = true;
    }
}

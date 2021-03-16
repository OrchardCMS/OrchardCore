using System.ComponentModel;

namespace OrchardCore.ContentFields.Settings
{
    public class HtmlFieldSettings
    {
        public string Hint { get; set; }

        [DefaultValue(true)]
        public bool SanitizeHtml { get; set; } = true;
    }
}

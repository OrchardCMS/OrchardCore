using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OrchardCore.Menu.Settings
{
    public class HtmlMenuItemPartSettings
    {
        /// <summary>
        /// Whether to sanitize the html input
        /// </summary>
        [DefaultValue(true)]
        public bool SanitizeHtml { get; set; } = true;
    }
}

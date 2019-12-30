using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayDriverOption
    {
        public ContentFieldDisplayDriverOption(Type displayDriverType)
        {
            DisplayDriverType = displayDriverType;
        }

        public Type DisplayDriverType { get; }


        /// <summary>
        /// Configures the display modes that this display driver will resolve.
        /// Valid options: * display for all display modes, String.Empty display for standard display mode, 'editor-name' display for specific display mode.
        /// </summary>
        public HashSet<string> DisplayModes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Configures the editors that this display driver will resolve.
        /// Valid options: * display for all editors, String.Empty display for standard editor, 'editor-name' display for specific editor.
        /// </summary>
        public HashSet<string> Editors { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}

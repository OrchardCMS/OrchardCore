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
        /// Valid options: * display for all display modes, 'standard' for standard display mode, 'display-name' for specific display mode.
        /// </summary>
        public HashSet<string> DisplayModes { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Configures the editors that this display driver will resolve.
        /// Valid options: * display for all editors, 'standard' for standard editor, 'editor-name' for specific editor.
        /// </summary>
        public HashSet<string> Editors { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static class ContentFieldDisplayDriverOptionExtensions
    {
        public static void ForDisplayModes(this ContentFieldDisplayDriverOption option, params string[] displayModes)
        {
            foreach (var displayMode in displayModes)
            {
                option.DisplayModes.Add(displayMode);
            }
        }

        public static void ForEditors(this ContentFieldDisplayDriverOption option, params string[] editors)
        {
            foreach (var editor in editors)
            {
                option.Editors.Add(editor);
            }
        }
    }
}

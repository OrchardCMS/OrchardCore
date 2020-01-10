using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayDriverOption
    {
        public ContentPartDisplayDriverOption(Type displayDriverType)
        {
            DisplayDriverType = displayDriverType;
        }

        public Type DisplayDriverType { get; }

        /// <summary>
        /// Configures the editors that this display driver will resolve.
        /// Valid options: * display for all editors, 'standard' for standard editor, 'editor-name' display for specific editor.
        /// </summary>
        public HashSet<string> Editors { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static class ContentPartDisplayDriverOptionExtensions
    {
        public static void ForEditors(this ContentPartDisplayDriverOption option, params string[] editors)
        {
            foreach (var editor in editors)
            {
                option.Editors.Add(editor);
            }
        }
    }
}

using System;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayDriverOption
    {
        public ContentPartDisplayDriverOption(Type displayDriverType)
        {
            DisplayDriverType = displayDriverType;
        }

        public Type DisplayDriverType { get; }

        public Func<string, bool> DisplayMode { get; private set; }

        public Func<string, bool> Editor { get; private set; }

        internal void SetDisplayMode(Func<string, bool> displayMode)
        {
            DisplayMode = displayMode;
        }

        internal void SetEditor(Func<string, bool> editor)
        {
            Editor = editor;
        }
    }
}

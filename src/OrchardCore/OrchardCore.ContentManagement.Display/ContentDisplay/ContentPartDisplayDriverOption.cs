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

        public Func<bool> Display { get; private set; }

        public Func<string, bool> Editor { get; private set; }

        internal void SetDisplay(Func<bool> displat)
        {
            Display = displat;
        }

        internal void SetEditor(Func<string, bool> editor)
        {
            Editor = editor;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayOption : ContentFieldOptionBase
    {
        private readonly List<ContentFieldDisplayDriverOption> _fieldDisplayDrivers = new();

        public ContentFieldDisplayOption(Type contentFieldType) : base(contentFieldType)
        {
        }

        private List<ContentFieldDisplayDriverOption> _displayModeDrivers;
        public IReadOnlyList<ContentFieldDisplayDriverOption> DisplayModeDrivers => _displayModeDrivers ??= _fieldDisplayDrivers.Where(d => d.DisplayMode != null).ToList();

        private List<ContentFieldDisplayDriverOption> _editorDrivers;
        public IReadOnlyList<ContentFieldDisplayDriverOption> EditorDrivers => _editorDrivers ??= _fieldDisplayDrivers.Where(d => d.Editor != null).ToList();

        internal void ForDisplayMode(Type displayDriverType, Func<string, bool> predicate)
        {
            var option = GetOrAddContentFieldDisplayDriverOption(displayDriverType);

            option.SetDisplayMode(predicate);
        }

        internal void ForEditor(Type displayDriverType, Func<string, bool> predicate)
        {
            var option = GetOrAddContentFieldDisplayDriverOption(displayDriverType);

            option.SetEditor(predicate);
        }

        internal void RemoveDisplayDriver(Type displayDriverType)
        {
            var displayDriverOption = _fieldDisplayDrivers.FirstOrDefault(d => d.DisplayDriverType == displayDriverType);
            if (displayDriverOption != null)
            {
                _fieldDisplayDrivers.Remove(displayDriverOption);
            }
        }

        private ContentFieldDisplayDriverOption GetOrAddContentFieldDisplayDriverOption(Type displayDriverType)
        {
            var option = _fieldDisplayDrivers.FirstOrDefault(o => o.DisplayDriverType == displayDriverType);

            if (option == null)
            {
                option = new ContentFieldDisplayDriverOption(displayDriverType);
                _fieldDisplayDrivers.Add(option);
            }

            return option;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayOption : ContentPartOptionBase
    {
        private readonly List<ContentPartDisplayDriverOption> _partDisplayDrivers = new();

        public ContentPartDisplayOption(Type contentPartType) : base(contentPartType)
        {
        }

        private List<ContentPartDisplayDriverOption> _displayDrivers;
        public IReadOnlyList<ContentPartDisplayDriverOption> DisplayDrivers => _displayDrivers ??= _partDisplayDrivers.Where(d => d.DisplayMode != null).ToList();

        private List<ContentPartDisplayDriverOption> _editorDrivers;
        public IReadOnlyList<ContentPartDisplayDriverOption> EditorDrivers => _editorDrivers ??= _partDisplayDrivers.Where(d => d.Editor != null).ToList();

        internal void ForDisplayMode(Type displayDriverType, Func<string, bool> predicate)
        {
            var option = GetOrAddContentPartDisplayDriverOption(displayDriverType);

            option.SetDisplayMode(predicate);
        }

        internal void ForEditor(Type displayDriverType, Func<string, bool> predicate)
        {
            var option = GetOrAddContentPartDisplayDriverOption(displayDriverType);

            option.SetEditor(predicate);
        }

        internal void RemoveDisplayDriver(Type displayDriverType)
        {
            var displayDriverOption = _partDisplayDrivers.FirstOrDefault(d => d.DisplayDriverType == displayDriverType);
            if (displayDriverOption != null)
            {
                _partDisplayDrivers.Remove(displayDriverOption);
            }
        }

        private ContentPartDisplayDriverOption GetOrAddContentPartDisplayDriverOption(Type displayDriverType)
        {
            var option = _partDisplayDrivers.FirstOrDefault(o => o.DisplayDriverType == displayDriverType);

            if (option == null)
            {
                option = new ContentPartDisplayDriverOption(displayDriverType);
                _partDisplayDrivers.Add(option);
            }

            return option;
        }
    }
}

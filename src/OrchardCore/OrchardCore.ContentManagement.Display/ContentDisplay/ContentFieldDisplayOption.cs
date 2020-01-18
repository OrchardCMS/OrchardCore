using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayOption : ContentFieldOptionBase
    {
        private readonly List<ContentFieldDisplayDriverOption> _displayDrivers = new List<ContentFieldDisplayDriverOption>();

        public ContentFieldDisplayOption(Type contentFieldType) : base(contentFieldType) { }

        public IReadOnlyList<ContentFieldDisplayDriverOption> DisplayDrivers => _displayDrivers;

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

        private ContentFieldDisplayDriverOption GetOrAddContentFieldDisplayDriverOption(Type displayDriverType)
        {
            var option = _displayDrivers.FirstOrDefault(o => o.DisplayDriverType == displayDriverType);

            if (option == null)
            {
                option = new ContentFieldDisplayDriverOption(displayDriverType);
                _displayDrivers.Add(option);
            }

            return option;
        }
    }
}

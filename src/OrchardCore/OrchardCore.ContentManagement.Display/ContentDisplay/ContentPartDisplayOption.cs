using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayOption : ContentPartOptionBase
    {
        private readonly List<ContentPartDisplayDriverOption> _displayDrivers = new List<ContentPartDisplayDriverOption>();

        public ContentPartDisplayOption(Type contentPartType) : base(contentPartType) { }

        public IReadOnlyList<ContentPartDisplayDriverOption> DisplayDrivers => _displayDrivers;

        internal void ForDisplay(Type displayDriverType, Func<bool> predicate)
        {
            var option = GetOrAddContentPartDisplayDriverOption(displayDriverType);

            option.SetDisplay(predicate);
        }

        internal void ForEditor(Type displayDriverType, Func<string, bool> predicate)
        {
            var option = GetOrAddContentPartDisplayDriverOption(displayDriverType);

            option.SetEditor(predicate);
        }

        private ContentPartDisplayDriverOption GetOrAddContentPartDisplayDriverOption(Type displayDriverType)
        {
            var option = _displayDrivers.FirstOrDefault(o => o.DisplayDriverType == displayDriverType);

            if (option == null)
            {
                option = new ContentPartDisplayDriverOption(displayDriverType);
                _displayDrivers.Add(option);
            }

            return option;
        }
    }
}

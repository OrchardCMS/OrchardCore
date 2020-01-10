using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentFieldDisplayOption : ContentFieldOptionBase
    {
        public ContentFieldDisplayOption(Type contentFieldType) : base(contentFieldType) { }

        public IList<ContentFieldDisplayDriverOption> DisplayDrivers = new List<ContentFieldDisplayDriverOption>();

        public void WithDisplayDriver(Type displayDriverType)
        {
            var option = new ContentFieldDisplayDriverOption(displayDriverType);
            option.DisplayModes.Add("*");
            option.Editors.Add("*");
            DisplayDrivers.Add(option);
        }

        public void WithDisplayDriver(Type displayDriverType, Action<ContentFieldDisplayDriverOption> action)
        {
            var option = new ContentFieldDisplayDriverOption(displayDriverType);
            action.Invoke(option);
            DisplayDrivers.Add(option);
        }
    }
}

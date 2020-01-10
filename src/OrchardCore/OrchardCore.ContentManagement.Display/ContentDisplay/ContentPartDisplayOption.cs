using System;
using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public class ContentPartDisplayOption : ContentPartOptionBase
    {
        public ContentPartDisplayOption(Type contentPartType) : base(contentPartType) { }

        public IList<ContentPartDisplayDriverOption> DisplayDrivers = new List<ContentPartDisplayDriverOption>();

        public void WithDisplayDriver(Type displayDriverType)
        {
            var option = new ContentPartDisplayDriverOption(displayDriverType);
            option.Editors.Add("*");
            DisplayDrivers.Add(option);
        }

        public void WithDisplayDriver(Type displayDriverType, string[] editors)
        {
            var option = new ContentPartDisplayDriverOption(displayDriverType);
            if (editors != null)
            {
                option.Editors = new HashSet<string>(editors, StringComparer.OrdinalIgnoreCase);
            }

            DisplayDrivers.Add(option);
        }

        public void WithDisplayDriver(Type displayDriverType, Action<ContentPartDisplayDriverOption> action)
        {
            var option = new ContentPartDisplayDriverOption(displayDriverType);
            action.Invoke(option);
            DisplayDrivers.Add(option);
        }
    }
}

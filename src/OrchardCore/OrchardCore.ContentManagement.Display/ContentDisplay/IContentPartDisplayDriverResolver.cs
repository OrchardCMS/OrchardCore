using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IList<IContentPartDisplayDriver> GetDisplayDrivers(string partName);
        IList<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor);
    }
}

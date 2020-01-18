using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IEnumerable<IContentPartDisplayDriver> GetDisplayDrivers(string partName);
        IEnumerable<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor);
    }
}

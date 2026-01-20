using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IList<IContentPartDisplayDriver> GetDisplayModeDrivers(string partName, string displayMode);
        IList<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor);
    }
}

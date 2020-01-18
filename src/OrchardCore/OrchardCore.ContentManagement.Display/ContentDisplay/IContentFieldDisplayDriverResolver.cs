using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriverResolver
    {
        IEnumerable<IContentFieldDisplayDriver> GetDisplayModeDrivers(string fieldName, string displayMode);
        IEnumerable<IContentFieldDisplayDriver> GetEditorDrivers(string fieldName, string editor);
    }
}

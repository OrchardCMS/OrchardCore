using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriverResolver
    {
        IReadOnlyList<IContentPartDisplayDriver> GetDriversForDisplay(string partName);
        IReadOnlyList<IContentPartDisplayDriver> GetDriversForEdit(string partName, string editor);
    }
}

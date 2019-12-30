using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriverResolver
    {
        IReadOnlyList<IContentFieldDisplayDriver> GetDriversForDisplay(string fieldName, string displayMode);
        IReadOnlyList<IContentFieldDisplayDriver> GetDriversForEdit(string fieldName, string editor);
    }
}

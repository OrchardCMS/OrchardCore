using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public interface IContentFieldDisplayDriverResolver
    {
        IReadOnlyList<IContentFieldDisplayDriver> GetDisplayDrivers(string partName);
    }
}

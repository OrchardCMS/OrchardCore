using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Handlers
{
    public interface IContentPartHandlerResolver
    {
        IReadOnlyList<IContentPartHandler> GetHandlers(string partName);
    }
}

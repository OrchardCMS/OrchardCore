using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Handlers
{
    public interface IContentFieldHandlerResolver
    {
        IList<IContentFieldHandler> GetHandlers(string fieldName);
    }
}

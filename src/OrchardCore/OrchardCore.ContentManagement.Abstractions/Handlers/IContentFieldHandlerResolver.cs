namespace OrchardCore.ContentManagement.Handlers;

public interface IContentFieldHandlerResolver
{
    IList<IContentFieldHandler> GetHandlers(string fieldName);
}

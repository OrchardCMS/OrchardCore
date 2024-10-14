namespace OrchardCore.ContentManagement.Handlers;

public interface IContentPartHandlerResolver
{
    IList<IContentPartHandler> GetHandlers(string partName);
}

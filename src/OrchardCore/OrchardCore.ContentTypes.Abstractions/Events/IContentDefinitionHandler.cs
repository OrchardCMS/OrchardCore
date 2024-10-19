namespace OrchardCore.ContentTypes.Events;

public interface IContentDefinitionHandler
{
    void LoadingContentType(LoadingContentTypeContext context);

    void LoadingContentTypePart(LoadingContentTypePartContext context);

    void LoadingContentPartField(LoadingContentPartFieldContext context);
}

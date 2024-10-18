namespace OrchardCore.ContentTypes.Events;

public interface IContentDefinitionHandler
{
    void TypeLoaded(LoadedContentTypeContext context);

    void TypePartLoaded(LoadedContentTypePartContext context);

    void PartFieldLoaded(LoadedContentPartFieldContext context);
}

namespace OrchardCore.ContentTypes.Events;

public interface IContentDefinitionHandler
{
    void BuildingContentType(BuildingContentTypeContext context);

    void BuildingContentTypePart(BuildingContentTypePartContext context);

    void BuildingContentPartField(BuildingContentPartFieldContext context);

    void BuildingContentPartDefinition(BuildingContentPartDefinitionContext context);
}

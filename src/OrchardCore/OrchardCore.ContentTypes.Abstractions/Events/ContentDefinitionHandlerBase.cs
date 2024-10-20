namespace OrchardCore.ContentTypes.Events;

public abstract class ContentDefinitionHandlerBase : IContentDefinitionHandler
{
    public virtual void BuildingContentPartDefinition(BuildingContentPartDefinitionContext context)
    {
    }

    public virtual void BuildingContentPartField(BuildingContentPartFieldContext context)
    {
    }

    public virtual void BuildingContentType(BuildingContentTypeContext context)
    {
    }

    public virtual void BuildingContentTypePart(BuildingContentTypePartContext context)
    {
    }
}

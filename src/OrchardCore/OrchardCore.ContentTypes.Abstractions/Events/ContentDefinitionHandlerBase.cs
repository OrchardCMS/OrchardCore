namespace OrchardCore.ContentTypes.Events;

public abstract class ContentDefinitionHandlerBase : IContentDefinitionEventHandler
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

    public virtual void ContentFieldAttached(ContentFieldAttachedContext context)
    {
    }

    public virtual void ContentFieldDetached(ContentFieldDetachedContext context)
    {
    }

    public virtual void ContentFieldUpdated(ContentFieldUpdatedContext context)
    {
    }

    public virtual void ContentPartAttached(ContentPartAttachedContext context)
    {
    }

    public virtual void ContentPartCreated(ContentPartCreatedContext context)
    {
    }

    public virtual void ContentPartDetached(ContentPartDetachedContext context)
    {
    }

    public virtual void ContentPartFieldUpdated(ContentPartFieldUpdatedContext context)
    {
    }

    public virtual void ContentPartImported(ContentPartImportedContext context)
    {
    }

    public virtual void ContentPartImporting(ContentPartImportingContext context)
    {
    }

    public virtual void ContentPartRemoved(ContentPartRemovedContext context)
    {
    }

    public virtual void ContentPartUpdated(ContentPartUpdatedContext context)
    {
    }

    public virtual void ContentTypeCreated(ContentTypeCreatedContext context)
    {
    }

    public virtual void ContentTypeImported(ContentTypeImportedContext context)
    {
    }

    public virtual void ContentTypeImporting(ContentTypeImportingContext context)
    {
    }

    public virtual void ContentTypePartUpdated(ContentTypePartUpdatedContext context)
    {
    }

    public virtual void ContentTypeRemoved(ContentTypeRemovedContext context)
    {
    }

    public virtual void ContentTypeUpdated(ContentTypeUpdatedContext context)
    {
    }
}

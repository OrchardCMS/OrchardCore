namespace OrchardCore.ContentManagement;

public abstract class ContentPartOptionBase
{
    public ContentPartOptionBase(Type contentPartType)
    {
        ArgumentNullException.ThrowIfNull(contentPartType);

        Type = contentPartType;
    }

    public Type Type { get; }
}

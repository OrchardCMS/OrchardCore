namespace OrchardCore.ContentManagement;

public abstract class ContentFieldOptionBase
{
    public ContentFieldOptionBase(Type contentFieldType)
    {
        ArgumentNullException.ThrowIfNull(contentFieldType);

        Type = contentFieldType;
    }

    public Type Type { get; }
}

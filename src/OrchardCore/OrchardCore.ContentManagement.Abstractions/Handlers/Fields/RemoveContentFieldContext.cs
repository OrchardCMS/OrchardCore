namespace OrchardCore.ContentManagement.Handlers;

public class RemoveContentFieldContext : ContentFieldContextBase
{
    public RemoveContentFieldContext(ContentItem contentItem, bool noActiveVersionLeft = false)
        : base(contentItem)
    {
        NoActiveVersionLeft = noActiveVersionLeft;
    }

    public bool NoActiveVersionLeft { get; }
}

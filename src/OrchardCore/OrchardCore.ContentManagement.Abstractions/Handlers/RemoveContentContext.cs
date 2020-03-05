namespace OrchardCore.ContentManagement.Handlers
{
    public class RemoveContentContext : ContentContextBase
    {
        public RemoveContentContext(ContentItem contentItem, bool noActiveVersionLeft = false) : base(contentItem)
        {
            NoActiveVersionLeft = noActiveVersionLeft;
        }

        public bool NoActiveVersionLeft { get; }
    }
}

namespace OrchardCore.ContentTypes.Events
{
    public class ContentFieldAttachedContext : ContentPartFieldContext
    {
        public string ContentFieldTypeName { get; set; }
        public string ContentFieldDisplayName { get; set; }
    }
}

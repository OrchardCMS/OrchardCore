namespace OrchardCore.ContentManagement.Metadata.Models
{
    public class ContentFieldDefinition
    {
        public ContentFieldDefinition(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}

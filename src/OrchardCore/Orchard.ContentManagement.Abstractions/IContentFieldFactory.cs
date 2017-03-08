using Orchard.ContentManagement;

public interface IContentFieldFactory
{
    ContentField CreateContentField(string fieldName);
}
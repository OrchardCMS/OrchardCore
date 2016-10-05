using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public interface IContentFieldDriver
    {
        ContentFieldInfo GetFieldInfo();
    }
}
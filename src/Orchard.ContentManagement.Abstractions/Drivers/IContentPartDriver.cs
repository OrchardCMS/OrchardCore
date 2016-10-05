using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public interface IContentPartDriver
    {
        ContentPartInfo GetPartInfo();
    }
}
using Orchard.ContentManagement.MetaData;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.Drivers
{
    public interface IContentPartDriver : IDependency
    {
        ContentPartInfo GetPartInfo();
    }
}
using Orchard.ContentManagement.MetaData;
using Orchard.DependencyInjection;

namespace Orchard.ContentManagement.Drivers
{
    public interface IContentFieldDriver : IDependency
    {
        ContentFieldInfo GetFieldInfo();
    }
}
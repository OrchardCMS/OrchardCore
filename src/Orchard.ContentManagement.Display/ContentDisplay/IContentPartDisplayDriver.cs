using Orchard.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using System;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentPartDisplayDriver : IDisplayDriver<ContentPart>, IDependency
    {
    }
}

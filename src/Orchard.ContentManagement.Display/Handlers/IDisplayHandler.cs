using Orchard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Handlers
{
    public interface IDisplayHandler : IDependency
    {
        Task BuildDisplayAsync(BuildDisplayContext context);
        Task BuildEditorAsync(BuildEditorContext context);
        Task UpdateEditorAsync(UpdateEditorContext context);
    }
}

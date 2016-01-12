using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.Display.Views;
using Orchard.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentElementDisplay : IDependency
    {
        DisplayResult BuildDisplay(BuildDisplayContext context);
        DisplayResult BuildEditor(BuildEditorContext context);
        Task UpdateEditorAsync(UpdateEditorContext context, IModelUpdater updater);
    }
    
}

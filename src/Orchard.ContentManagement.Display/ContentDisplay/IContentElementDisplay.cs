using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.Display.Views;
using Orchard.DependencyInjection;
using System;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public interface IContentElementDisplay : IDependency
    {
        DisplayResult BuildDisplay(BuildDisplayContext context);
        DisplayResult BuildEditor(BuildEditorContext context);
        DisplayResult UpdateEditor(UpdateEditorContext context);
    }
    
}

using Orchard.DisplayManagement.Handlers;

namespace Orchard.DisplayManagement.Views
{
    public interface IDisplayResult
    {
        void Apply(BuildDisplayContext context);
        void Apply(BuildEditorContext context);
    }
}

using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.DisplayManagement.Views
{
    public interface IDisplayResult
    {
        void Apply(BuildDisplayContext context);
        void Apply(BuildEditorContext context);
    }
}

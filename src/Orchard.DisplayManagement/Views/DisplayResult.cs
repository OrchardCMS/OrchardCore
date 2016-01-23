using Orchard.DisplayManagement.Handlers;

namespace Orchard.DisplayManagement.Views
{
    public class DisplayResult<TModel>
    {
        public virtual void Apply(BuildDisplayContext<TModel> context) { }
        public virtual void Apply(BuildEditorContext<TModel> context) { }
    }
}

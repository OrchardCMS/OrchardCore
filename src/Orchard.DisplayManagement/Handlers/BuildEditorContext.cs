namespace Orchard.DisplayManagement.Handlers
{
    public class BuildEditorContext<TModel> : BuildShapeContext<TModel>
    {
        public BuildEditorContext(IShape model, TModel content, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory)
        {
        }
    }
}

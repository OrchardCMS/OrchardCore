namespace Orchard.DisplayManagement.Handlers
{
    public class UpdateEditorContext<TModel> : BuildEditorContext<TModel>
    {
        public UpdateEditorContext(IShape model, TModel content, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory)
        {
        }
    }
}

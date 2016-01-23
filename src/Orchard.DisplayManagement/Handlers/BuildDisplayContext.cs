namespace Orchard.DisplayManagement.Handlers
{
    public class BuildDisplayContext<TModel> : BuildShapeContext<TModel>
    {
        public BuildDisplayContext(IShape model, TModel content, string displayType, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory)
        {
            DisplayType = displayType;
        }

        public string DisplayType { get; private set; }

    }
}

using Orchard.DisplayManagement.Descriptors;
using System;

namespace Orchard.DisplayManagement.Handlers
{
    public class BuildShapeContext<TModel>
    {
        protected BuildShapeContext(IShape shape, TModel model, string groupId, IShapeFactory shapeFactory)
        {
            Shape = shape;
            Model = model;
            ShapeFactory = shapeFactory;
            GroupId = groupId;
            FindPlacement = (partType, differentiator, defaultLocation) => new PlacementInfo { Location = defaultLocation, Source = String.Empty };
        }

        public dynamic Shape { get; private set; }
        public TModel Model { get; private set; }
        public IShapeFactory ShapeFactory { get; private set; }
        public dynamic New => ShapeFactory;
        public IShape Layout { get; set; }
        public string GroupId { get; private set; }
        public Func<string, string, string, PlacementInfo> FindPlacement { get; set; }
    }
}

using Orchard.DisplayManagement.Descriptors;
using System;

namespace Orchard.DisplayManagement.Handlers
{
    public abstract class BuildShapeContext : IBuildShapeContext
    {
        protected BuildShapeContext(IShape shape, string groupId, IShapeFactory shapeFactory, IShape layout)
        {
            Shape = shape;
            ShapeFactory = shapeFactory;
            GroupId = groupId;
            Layout = layout;
            FindPlacement = FindDefaultPlacement;
        }

        public dynamic Shape { get; private set; }
        public IShapeFactory ShapeFactory { get; private set; }
        public dynamic New => ShapeFactory;
        public IShape Layout { get; set; }
        public string GroupId { get; private set; }
        public Func<string, string, string, PlacementInfo> FindPlacement { get; set; }

        private PlacementInfo FindDefaultPlacement(string partType, string differentiator, string defaultLocation)
        {
            return new PlacementInfo { Location = defaultLocation, Source = String.Empty };
        }
    }
}

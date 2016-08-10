using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.ModelBinding;
using System;

namespace Orchard.DisplayManagement.Handlers
{
    public abstract class BuildShapeContext : IBuildShapeContext
    {
        protected BuildShapeContext(IShape shape, string groupId, IShapeFactory shapeFactory, IShape layout, IUpdateModel updater)
        {
            Shape = shape;
            ShapeFactory = shapeFactory;
            GroupId = groupId;
            Layout = layout;
            FindPlacement = FindDefaultPlacement;
            Updater = updater;
        }

        public IShape Shape { get; private set; }
        public IShapeFactory ShapeFactory { get; private set; }
        public dynamic New => ShapeFactory;
        public IShape Layout { get; set; }
        public string GroupId { get; private set; }
        public FindPlacementDelegate FindPlacement { get; set; }
        public IUpdateModel Updater { get; }

        private PlacementInfo FindDefaultPlacement(IShape shape, string differentiator, string defaultLocation)
        {
            return new PlacementInfo { Location = defaultLocation, Source = String.Empty };
        }
    }
}

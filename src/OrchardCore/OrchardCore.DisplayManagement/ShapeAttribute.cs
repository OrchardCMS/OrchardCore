using System;

namespace OrchardCore.DisplayManagement
{
    public class ShapeAttribute : Attribute
    {
        public ShapeAttribute()
        {
        }

        public ShapeAttribute(string shapeType)
        {
            this.ShapeType = shapeType;
        }

        public string ShapeType { get; private set; }
    }
}
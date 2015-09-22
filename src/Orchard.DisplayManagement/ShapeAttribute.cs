using System;

namespace Orchard.DisplayManagement {
    public class ShapeAttribute : Attribute {
        public ShapeAttribute() { }
        public ShapeAttribute(string shapeType) { ShapeType = shapeType; }
        
        public string ShapeType { get; private set; }
    }
}
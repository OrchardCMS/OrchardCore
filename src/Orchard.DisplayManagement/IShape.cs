using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement {
    /// <summary>
    /// Interface present on dynamic shapes. 
    /// May be used to access attributes in a strongly typed fashion.
    /// Note: Anything on this interface is a reserved word for the purpose of shape properties
    /// </summary>    
    public interface IShape {
        ShapeMetadata Metadata { get; set; }
    }
}
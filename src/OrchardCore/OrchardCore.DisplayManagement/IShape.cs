using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement
{
    /// <summary>
    /// Interface present on dynamic shapes.
    /// May be used to access attributes in a strongly typed fashion.
    /// Note: Anything on this interface is a reserved word for the purpose of shape properties
    /// </summary>
    public interface IShape
    {
        ShapeMetadata Metadata { get; }
        string Id { get; set; }
        IList<string> Classes { get; }
        IDictionary<string, string> Attributes { get; }
        IDictionary<string, object> Properties { get; }
	}

    //public static class IShapeExtensions
    //{
    //    public static JObject ShapeDump(this IShape shape)
    //    {
    //        var jObject = new JObject();
    //        jObject.Add("ShapeType", JToken.FromObject(shape.Metadata.DisplayType));
    //        return jObject;
    //    }
    //}
}

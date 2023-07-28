using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.DisplayManagement.Shapes
{
    /// <summary>
    /// Safely serializes a shape to Json.
    /// </summary>
    /// <remarks>
    /// A new instance should be created for each <see cref="IShape"/>
    /// </remarks>
    public class ShapeSerializer
    {
        // This code is used for debugging so does not need to be performance optimized.
        private static readonly JsonSerializer _shapeJsonSerializer = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private readonly HashSet<IShape> _shapes = new();
        private readonly IShape _shape;

        public ShapeSerializer(IShape shape)
        {
            _shape = shape;
        }

        public JObject Serialize()
        {
            if (_shape == null)
            {
                return new JObject();
            }

            var jObject = new JObject();

            // Provides a convenient identifier in console.
            var displayText = _shape.Metadata.Name;
            if (String.IsNullOrEmpty(displayText))
            {
                displayText = _shape.Metadata.Type;
            }

            if (String.IsNullOrEmpty(displayText))
            {
                displayText = _shape.GetType().Name;
            }

            jObject.Add("Shape", displayText);

            var metadata = JObject.FromObject(_shape.Metadata, _shapeJsonSerializer);

            jObject.Add(nameof(ShapeMetadata), metadata);

            if (_shape.Classes != null && _shape.Classes.Any())
            {
                jObject.Add(nameof(_shape.Classes), JArray.FromObject(_shape.Classes, _shapeJsonSerializer));
            }

            if (_shape.Attributes != null && _shape.Attributes.Any())
            {
                jObject.Add(nameof(_shape.Attributes), JObject.FromObject(_shape.Attributes, _shapeJsonSerializer));
            }

            if (_shape.Properties != null && _shape.Properties.Any())
            {
                jObject.Add(nameof(_shape.Properties), JObject.FromObject(_shape.Properties, _shapeJsonSerializer));
                FindShapesInProperties(_shape);
            }

            if (_shape is Shape actualShape && actualShape.HasItems && _shapes.Add(actualShape))
            {
                var jItems = new JArray();
                // Because items can be mutated during shape execution.
                var shapeItems = actualShape.Items.ToArray();
                foreach (var item in shapeItems)
                {
                    if (item is IShape shapeItem && _shapes.Add(shapeItem))
                    {
                        // Display item in json so source of item remains clear.
                        var jItem = shapeItem.ShapeToJson();
                        jItems.Add(jItem);
                    }
                }
                jObject.Add(nameof(actualShape.Items), jItems);
            }

            return jObject;
        }

        private void FindShapesInProperties(IShape shape)
        {
            foreach (var property in shape.Properties.Values)
            {
                if (property is Shape shapeProperty && _shapes.Add(shapeProperty) && shapeProperty.HasItems)
                {
                    var shapeItems = shapeProperty.Items.ToArray();
                    foreach (var item in shapeItems.Cast<IShape>())
                    {
                        if (item is IShape shapeItem && _shapes.Add(shapeItem))
                        {
                            // Recurse for more shapes.
                            FindShapesInProperties(shapeItem);
                        }
                    }
                }
            }
        }
    }
}

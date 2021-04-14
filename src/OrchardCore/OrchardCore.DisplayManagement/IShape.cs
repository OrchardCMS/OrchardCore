using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

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
        string TagName { get; set; }
        IList<string> Classes { get; }
        IDictionary<string, string> Attributes { get; }
        IDictionary<string, object> Properties { get; }
        IReadOnlyList<IPositioned> Items { get; }
        ValueTask<IShape> AddAsync(object item, string position);
    }

    public static class IShapeExtensions
    {
        public static bool IsNullOrEmpty(this IShape shape) => shape == null || shape is ZoneOnDemand;

        public static bool TryGetProperty<T>(this IShape shape, string key, out T value)
        {
            if (shape.Properties != null && shape.Properties.TryGetValue(key, out var result))
            {
                if (result is T t)
                {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static object GetProperty(this IShape shape, string key)
        {
            return GetProperty(shape, key, (object)null);
        }

        public static T GetProperty<T>(this IShape shape, string key)
        {
            return GetProperty(shape, key, default(T));
        }

        public static T GetProperty<T>(this IShape shape, string key, T value)
        {
            if (shape.Properties != null && shape.Properties.TryGetValue(key, out var result))
            {
                if (result is T t)
                {
                    return t;
                }
            }

            return value;
        }

        public static async ValueTask<IShape> AddRangeAsync(this IShape shape, IEnumerable<object> items, string position = null)
        {
            foreach (var item in items)
            {
                await shape.AddAsync(item, position);
            }

            return shape;
        }

        public static ValueTask<IShape> AddAsync(this IShape shape, object item)
        {
            return shape.AddAsync(item, "");
        }

        public static TagBuilder GetTagBuilder(this IShape shape, string defaultTagName = "span")
        {
            var tagName = defaultTagName;

            // We keep this for backward compatibility
            if (shape.TryGetProperty("Tag", out string valueString))
            {
                tagName = valueString;
            }

            if (!String.IsNullOrEmpty(shape.TagName))
            {
                tagName = shape.TagName;
            }

            var tagBuilder = new TagBuilder(tagName);

            if (shape.Attributes != null)
            {
                tagBuilder.MergeAttributes(shape.Attributes, false);
            }

            if (shape.Classes != null)
            {
                foreach (var cssClass in shape.Classes)
                {
                    tagBuilder.AddCssClass(cssClass);
                }
            }

            if (!String.IsNullOrWhiteSpace(shape.Id))
            {
                tagBuilder.Attributes["id"] = shape.Id;
            }

            return tagBuilder;
        }

        private static readonly JsonSerializer ShapeSerializer = new JsonSerializer
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static JObject ShapeToJson(this IShape shape)
        {
            if (shape == null)
            {
                return new JObject();
            }

            var jObject = new JObject();

            // Provides a convenient identifier in console.
            var displayText = shape.Metadata.Name;
            if (String.IsNullOrEmpty(displayText))
            {
                displayText = shape.Metadata.Type;
            }

            if (String.IsNullOrEmpty(displayText))
            {
                displayText = shape.GetType().Name;
            }

            jObject.Add("Shape", displayText);

            var metadata = JObject.FromObject(shape.Metadata, ShapeSerializer);

            jObject.Add(nameof(ShapeMetadata), metadata);

            if (shape.Classes != null && shape.Classes.Any())
            {
                jObject.Add(nameof(shape.Classes), JArray.FromObject(shape.Classes, ShapeSerializer));
            }

            if (shape.Attributes != null && shape.Attributes.Any())
            {
                jObject.Add(nameof(shape.Attributes), JObject.FromObject(shape.Attributes, ShapeSerializer));
            }

            if (shape.Properties != null && shape.Properties.Any())
            {
                jObject.Add(nameof(shape.Properties), JObject.FromObject(shape.Properties, ShapeSerializer));
                FindShapesInProperties(shape);
            }

            var actualShape = shape as Shape;
            if (actualShape != null && actualShape.HasItems)
            {
                var jItems = new JArray();
                // Because items can be mutated during shape execution.
                var shapeItems = actualShape.Items.ToArray();
                foreach (IShape item in shapeItems)
                {
                    // Display item in json so source of item remains clear.
                    var jItem = item.ShapeToJson();
                    jItems.Add(jItem);
                }
                jObject.Add(nameof(actualShape.Items), jItems);
            }

            return jObject;
        }

        private static void FindShapesInProperties(IShape shape)
        {
            foreach (var property in shape.Properties.Values)
            {
                var shapeProperty = property as Shape;
                if (shapeProperty != null && shapeProperty.HasItems)
                {
                    var shapeItems = shapeProperty.Items.ToArray();
                    foreach (IShape item in shapeItems)
                    {
                        // Recurse for more shapes.
                        FindShapesInProperties(item);
                    }
                }
            }
        }
    }
}

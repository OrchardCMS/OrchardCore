using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Shapes;
using System.Collections.Immutable;
using System;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace OrchardCore.DisplayManagement
{
    public static class IShapeExtensions
    {
        public static readonly JsonSerializer IgnoreDefaultValuesSerializer = new JsonSerializer {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = ShouldSerializeContractResolver.Instance

        };


        public static readonly JsonSerializer ReferenceLoopHandlingSerializer = new JsonSerializer { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        public static readonly JsonMergeSettings JsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union, MergeNullValueHandling = MergeNullValueHandling.Merge };


        public static JObject ShapeDump(this IShape shape)
        {
            if (shape == null)
            {
                return new JObject();
            }
            //return JArray.FromObject(shape, ReferenceLoopHandlingSerializer);
            var meta = JObject.FromObject(shape.Metadata, IgnoreDefaultValuesSerializer);
            var jObject = new JObject();
            if (meta.HasValues)
            {
                jObject.Add("Meta", meta);
            }

            //var metadata = new JObject();

            ////// Hmm which is better, more or less info. Or use JsonIgnore?
            //if (shape.Metadata?.Name != null)
            //{
            //    metadata.Add("Type", shape.Metadata.Type);
            //}
            //if (shape.Metadata?.Name != null)
            //{
            //    metadata.Add("Name", shape.Metadata.Name);
            //}

            //if (shape.Metadata?.DisplayType != null)
            //{
            //    metadata.Add(nameof(shape.Metadata.DisplayType), shape.Metadata.DisplayType);
            //}

            //if (shape.Metadata?.Alternates != null && shape.Metadata.Alternates.Any())
            //{
            //    jObject.Add(nameof(shape.Metadata.Alternates), JArray.FromObject(shape.Metadata.Alternates));
            //}

            //if (shape.Metadata?.Wrappers != null && shape.Metadata.Wrappers.Any())
            //{
            //    jObject.Add(nameof(shape.Metadata.Wrappers), JArray.FromObject(shape.Metadata.Wrappers));
            //}

            //if (metadata.HasValues)
            //{
            //    jObject.Add("ShapeMetadata", metadata);
            //}

            if (shape.Classes != null && shape.Classes.Any())
            {
                jObject.Add(nameof(shape.Classes), JArray.FromObject(shape.Classes, IgnoreDefaultValuesSerializer));
            }
            if (shape.Attributes != null && shape.Attributes.Any())
            {
                jObject.Add("Attributes", JObject.FromObject(shape.Attributes, IgnoreDefaultValuesSerializer));
            }
            if (shape.Properties != null && shape.Properties.Any())
            {
                jObject.Add("Properties", JObject.FromObject(shape.Properties, IgnoreDefaultValuesSerializer));
            }
            //if (shape.Properties != null && shape.Properties.Any())
            //{
            //    var properties = new JObject();
            //    var t = shape.Properties.ToImmutableDictionary();
            //    foreach(var property in t)
            //    {
            //        try
            //        {
            //            if (property.Value is IShape)
            //            {
            //                var s = property.Value as IShape;
            //                var obj = s.ShapeDump();
            //                properties.Add(property.Key, obj);
            //            }
            //            //else
            //            //{
            //            //    properties.Add(JProperty.FromObject(property));
            //            //}
            //        } catch (Exception e)
            //        {
            //            throw e;
            //        }
            //    }
            //    jObject.Add("Properties", properties);
            //}

            var dynamic = shape as Shape;
            if (dynamic != null && dynamic.HasItems)
            {
                var items = new JArray();
                // Because items can be mutated during shape execution.
                var dynamicItems = dynamic.Items.ToImmutableArray();
                foreach(var item in dynamicItems)
                {
                    var s = (IShape)item;
                    items.Add(s.ShapeDump());
                }
                jObject.Add("Items", items);
            }

            return jObject;// JObject.FromObject(jObject.Merge(JsonMergeSettings), IgnoreDefaultValuesSerializer);
        }
    }

    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType.GetInterface(nameof(IEnumerable)) != null)
                property.ShouldSerialize =
                    instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;

            return property;
        }
    }
}

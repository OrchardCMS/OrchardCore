using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Shapes
{
    [DebuggerTypeProxy(typeof(ShapeDebugView))]
    public class Shape : Composite, IShape, IPositioned, IEnumerable<object>
    {
        private List<string> _classes;
        private Dictionary<string, string> _attributes;
        private readonly List<IPositioned> _items = new List<IPositioned>();
        private bool _sorted = false;

        public ShapeMetadata Metadata { get; } = new ShapeMetadata();

        public string Id { get; set; }
        public string TagName { get; set; }
        public IList<string> Classes => _classes ??= new List<string>();
        public IDictionary<string, string> Attributes => _attributes ??= new Dictionary<string, string>();
        public IEnumerable<dynamic> Items
        {
            get
            {
                if (!_sorted)
                {
                    _items.Sort(FlatPositionComparer.Instance);
                    _sorted = true;
                }

                return _items;
            }
        }

        public bool HasItems => _items.Count > 0;

        public string Position
        {
            get { return Metadata.Position; }
            set { Metadata.Position = value; }
        }

        public virtual Shape Add(object item, string position = null)
        {
            if (item == null)
            {
                return this;
            }

            if (position == null)
            {
                position = "";
            }

            _sorted = false;

            if (item is IHtmlContent)
            {
                _items.Add(new PositionWrapper((IHtmlContent)item, position));
            }
            else if (item is string)
            {
                _items.Add(new PositionWrapper((string)item, position));
            }
            else
            {
                var shape = item as IPositioned;
                if (shape != null)
                {
                    if (position != null)
                    {
                        shape.Position = position;
                    }

                    _items.Add(shape);
                }
            }

            return this;
        }

        public Shape AddRange(IEnumerable<object> items, string position = null)
        {
            foreach (var item in items)
            {
                Add(item, position);
            }

            return this;
        }

        public void Remove(string shapeName)
        {
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i] is IShape shape && shape.Metadata.Name == shapeName)
                {
                    _items.RemoveAt(i);
                    return;
                }
            }
        }

        public IShape Named(string shapeName)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is IShape shape && shape.Metadata.Name == shapeName)
                {
                    return shape;
                }
            }

            return null;
        }

        public IShape NormalizedNamed(string shapeName)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i] is IShape shape && shape.Metadata.Name?.Replace("__", "-") == shapeName)
                {
                    return shape;
                }
            }

            return null;
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            if (!_sorted)
            {
                _items.Sort(FlatPositionComparer.Instance);
                _sorted = true;
            }

            return _items.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            if (!_sorted)
            {
                _items.Sort(FlatPositionComparer.Instance);
                _sorted = true;
            }

            return _items.GetEnumerator();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Items;

            if (binder.ReturnType == typeof(IEnumerable<object>) ||
                binder.ReturnType == typeof(IEnumerable<dynamic>))
            {
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public static TagBuilder GetTagBuilder(Shape shape, string defaultTagName = "span")
        {
            var tagName = defaultTagName;

            // We keep this for backward compatibility
            if (shape.Properties.TryGetValue("Tag", out var value) && value is string valueString)
            {
                tagName = valueString;
            }

            if (!String.IsNullOrEmpty(shape.TagName))
            {
                tagName = shape.TagName;
            }

            return GetTagBuilder(tagName, shape.Id, shape.Classes, shape.Attributes);
        }

        public static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes)
        {
            var tagBuilder = new TagBuilder(tagName);

            if (attributes != null)
            {
                tagBuilder.MergeAttributes(attributes, false);
            }

            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
            {
                tagBuilder.AddCssClass(cssClass);
            }

            if (!String.IsNullOrWhiteSpace(id))
            {
                tagBuilder.Attributes["id"] = id;
            }
            return tagBuilder;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name;

            if (!base.TryGetMember(binder, out result) || (null == result))
            {
                // Try to get a Named shape
                result = Named(name);

                if (result == null)
                {
                    result = NormalizedNamed(name.Replace("__", "-"));
                }
            }

            return true;
        }

        protected override bool TrySetMemberImpl(string name, object value)
        {
            // We set the Shape real properties for Razor
            if (name == "Id")
            {
                Id = value as string;

                return true;
            }

            if (name == "TagName")
            {
                TagName = value as string;

                return true;
            }

            if (name == "Attributes")
            {
                if (value is Dictionary<string, string> attributes)
                {
                    foreach (var attribute in attributes)
                    {
                        Attributes.TryAdd(attribute.Key, attribute.Value);
                    }
                }

                if (value is string stringValue)
                {
                    attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringValue);

                    foreach (var attribute in attributes)
                    {
                        Attributes.TryAdd(attribute.Key, attribute.Value);
                    }
                }
            }

            if (name == "Classes")
            {
                if (value is List<string> classes)
                {
                    foreach (var item in classes)
                    {
                        Classes.Add(item);
                    }
                }

                if (value is string stringValue)
                {
                    var values = stringValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in values)
                    {
                        Classes.Add(item);
                    }
                }
            }

            base.TrySetMemberImpl(name, value);

            return true;
        }
    }
}

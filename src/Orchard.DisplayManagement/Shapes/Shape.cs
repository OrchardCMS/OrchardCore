using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orchard.UI;

namespace Orchard.DisplayManagement.Shapes
{
    [DebuggerTypeProxy(typeof(ShapeDebugView))]
    public class Shape : Composite, IShape, IPositioned, IEnumerable<object>
    {
        private readonly IList<string> _classes = new List<string>();
        private readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();
        private readonly List<IPositioned> _items = new List<IPositioned>();
        private bool _sorted = false;
        public ShapeMetadata Metadata { get; set; }

        public string Id { get; set; }
        public IList<string> Classes => _classes;
        public IDictionary<string, string> Attributes => _attributes;
        public IEnumerable<dynamic> Items => _items;
        public bool HasItems => _items.Count > 0;
        public string Position
        {
            get { return Metadata.Position; }
            set { Metadata.Position = value; }
        }

        public Shape()
        {
            Metadata = new ShapeMetadata();
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


        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            if(!_sorted)
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

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1)
            {
                var name = indexes[0].ToString();
                if (name.Equals("Id"))
                {
                    // need to mutate the actual type
                    Id = Convert.ToString(value);
                    return true;
                }
                if (name.Equals("Classes"))
                {
                    var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                    MergeClasses(args, Classes);
                    return true;
                }
                if (name.Equals("Attributes"))
                {
                    var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                    MergeAttributes(args, Attributes);
                    return true;
                }
                if (name.Equals("Items"))
                {
                    var args = Arguments.From(new[] { value }, Enumerable.Empty<string>());
                    MergeItems(args, this);
                    return true;
                }
            }

            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;
            var arguments = Arguments.From(args, binder.CallInfo.ArgumentNames);
            if (name.Equals("Id"))
            {
                // need to mutate the actual type
                Id = Convert.ToString(args.FirstOrDefault());
                result = this;
                return true;
            }
            if (name.Equals("Classes") && !arguments.Named.Any())
            {
                MergeClasses(arguments, Classes);
                result = this;
                return true;
            }
            if (name.Equals("Attributes") && arguments.Positional.Count() <= 1)
            {
                MergeAttributes(arguments, Attributes);
                result = this;
                return true;
            }
            if (name.Equals("Items"))
            {
                MergeItems(arguments, this);
                result = this;
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public static TagBuilder GetTagBuilder(dynamic shape, string defaultTag = "span")
        {
            string tagName = shape.Tag;

            // Dont replace by ?? as shape.Tag is dynamic
            if (tagName == null)
            {
                tagName = defaultTag;
            }

            string id = shape.Id;
            IEnumerable<string> classes = shape.Classes;
            IDictionary<string, string> attributes = shape.Attributes;

            return GetTagBuilder(tagName, id, classes, attributes);
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

            if (!string.IsNullOrWhiteSpace(id))
            {
                tagBuilder.Attributes["id"] = id;
            }
            return tagBuilder;
        }

        private static void MergeAttributes(INamedEnumerable<object> args, IDictionary<string, string> attributes)
        {
            var arg = args.Positional.SingleOrDefault();
            if (arg != null)
            {
                if (arg is IDictionary)
                {
                    var dictionary = arg as IDictionary;
                    foreach (var key in dictionary.Keys)
                    {
                        attributes[Convert.ToString(key)] = Convert.ToString(dictionary[key]);
                    }
                }
                else
                {
                    foreach (var prop in arg.GetType().GetProperties())
                    {
                        attributes[TranslateIdentifier(prop.Name)] = Convert.ToString(prop.GetValue(arg, null));
                    }
                }
            }
            foreach (var named in args.Named)
            {
                attributes[named.Key] = Convert.ToString(named.Value);
            }
        }

        private static string TranslateIdentifier(string name)
        {
            // Allows for certain characters in an identifier to represent different
            // characters in an HTML attribute (mimics MVC behavior):
            // data_foo ==> data-foo
            // @keyword ==> keyword
            return name.Replace("_", "-").Replace("@", "");
        }

        private static void MergeClasses(INamedEnumerable<object> args, IList<string> classes)
        {
            foreach (var arg in args)
            {
                // look for string first, because the "string" type is also an IEnumerable of char
                if (arg is string)
                {
                    classes.Add(arg as string);
                }
                else if (arg is IEnumerable)
                {
                    foreach (var item in arg as IEnumerable)
                    {
                        classes.Add(Convert.ToString(item));
                    }
                }
                else
                {
                    classes.Add(Convert.ToString(arg));
                }
            }
        }

        private static void MergeItems(INamedEnumerable<object> args, dynamic shape)
        {
            foreach (var arg in args)
            {
                // look for string first, because the "string" type is also an IEnumerable of char
                if (arg is string)
                {
                    shape.Add(arg as string);
                }
                else if (arg is IEnumerable)
                {
                    foreach (var item in arg as IEnumerable)
                    {
                        shape.Add(item);
                    }
                }
                else
                {
                    shape.Add(Convert.ToString(arg));
                }
            }
        }
    }
}
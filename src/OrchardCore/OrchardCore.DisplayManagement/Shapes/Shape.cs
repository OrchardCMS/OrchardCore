using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Shapes
{
    [DebuggerTypeProxy(typeof(ShapeDebugView))]
    public class Shape : Composite, IShape, IPositioned, IEnumerable<object>
    {
        private bool _sorted = false;

        public ShapeMetadata Metadata { get; } = new ShapeMetadata();

        public string Id { get; set; }
        public string TagName { get; set; }

        private List<string> _classes;
        public IList<string> Classes => _classes ??= new List<string>();

        private Dictionary<string, string> _attributes;
        public IDictionary<string, string> Attributes => _attributes ??= new Dictionary<string, string>();

        private List<IPositioned> _items;
        public IReadOnlyList<IPositioned> Items
        {
            get
            {
                _items ??= new List<IPositioned>();

                if (!_sorted)
                {
                    _items = _items.OrderBy(x => x, FlatPositionComparer.Instance).ToList();
                    _sorted = true;
                }

                return _items;
            }
        }

        public bool HasItems => _items != null && _items.Count > 0;

        public string Position
        {
            get { return Metadata.Position; }
            set { Metadata.Position = value; }
        }

        public virtual ValueTask<IShape> AddAsync(object item, string position)
        {
            if (item == null)
            {
                return new ValueTask<IShape>(this);
            }

            position ??= "";

            _sorted = false;

            _items ??= new List<IPositioned>();

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

            return new ValueTask<IShape>(this);
        }

        public void Remove(string shapeName)
        {
            if (_items == null)
            {
                return;
            }

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
            if (_items == null)
            {
                return null;
            }

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
            if (_items == null)
            {
                return null;
            }

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
            if (_items == null)
            {
                return Enumerable.Empty<object>().GetEnumerator();
            }

            if (!_sorted)
            {
                _items = _items.OrderBy(x => x, FlatPositionComparer.Instance).ToList();
                _sorted = true;
            }

            return _items.GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            if (_items == null)
            {
                return Enumerable.Empty<object>().GetEnumerator();
            }

            if (!_sorted)
            {
                _items = _items.OrderBy(x => x, FlatPositionComparer.Instance).ToList();
                _sorted = true;
            }

            return _items.GetEnumerator();
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Items;

            if (binder.ReturnType == typeof(IEnumerable<object>))
            {
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            // In case AddAsync() is called on a dynamic object, to prevent Composite from seing it as a property assignment.
            if (binder.Name == "AddAsync")
            {
                result =
                    AddAsync(args.Length > 0 ? args[0] : null, args.Length > 1 ? args[1].ToString() : "")
                    .AsTask();

                return true;
            }

            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMemberImpl(binder.Name, out result);
        }

        protected override bool TryGetMemberImpl(string name, out object result)
        {
            if (!base.TryGetMemberImpl(name, out result) || (null == result))
            {
                // Try to get a Named shape.
                result = Named(name);

                result ??= NormalizedNamed(name.Replace("__", "-"));
            }

            return true;
        }

        protected override bool TrySetMemberImpl(string name, object value)
        {
            // We set the Shape real properties for Razor.

            if (name == "Id")
            {
                Id = value as string;

                return true;
            }
            else if (name == "TagName")
            {
                TagName = value as string;

                return true;
            }
            else if (name == "Attributes")
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
            else if (name == "Classes")
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

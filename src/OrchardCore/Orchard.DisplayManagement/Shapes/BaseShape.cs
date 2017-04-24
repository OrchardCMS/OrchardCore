using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Orchard.UI;

namespace Orchard.DisplayManagement.Shapes
{
    public class BaseShape : IShape, IPositioned, IEnumerable<object>
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

        public BaseShape()
        {
            Metadata = new ShapeMetadata();
        }

        public virtual IShape Add(object item, string position = null)
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

        public IShape AddRange(IEnumerable<object> items, string position = null)
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
    }
}
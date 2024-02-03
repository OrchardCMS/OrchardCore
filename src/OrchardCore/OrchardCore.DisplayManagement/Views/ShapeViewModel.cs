using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Views
{
    public class ShapeViewModel : IShape, IPositioned
    {
        public ShapeViewModel()
        {
        }

        public ShapeViewModel(string shapeType)
        {
            ArgumentException.ThrowIfNullOrEmpty(shapeType, nameof(shapeType));

            Metadata.Type = shapeType;
        }

        private ShapeMetadata _metadata;

        public ShapeMetadata Metadata => _metadata ??= new ShapeMetadata();

        public string Position
        {
            get => Metadata.Position;
            set
            {
                Metadata.Position = value;
            }
        }

        public string Id { get; set; }

        public string TagName { get; set; }

        private List<string> _classes;

        public IList<string> Classes => _classes ??= [];

        private Dictionary<string, string> _attributes;

        public IDictionary<string, string> Attributes => _attributes ??= [];

        private Dictionary<string, object> _properties;

        public IDictionary<string, object> Properties => _properties ??= [];

        private bool _sorted = false;

        private List<IPositioned> _items;

        public IReadOnlyList<IPositioned> Items
        {
            get
            {
                _items ??= [];

                if (!_sorted)
                {
                    _items = _items.OrderBy(x => x, FlatPositionComparer.Instance).ToList();
                    _sorted = true;
                }

                return _items;
            }
        }

        public ValueTask<IShape> AddAsync(object item, string position)
        {
            if (item == null)
            {
                return new ValueTask<IShape>(this);
            }

            position ??= string.Empty;
            _sorted = false;
            _items ??= [];

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
                if (item is IPositioned shape)
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
    }
}

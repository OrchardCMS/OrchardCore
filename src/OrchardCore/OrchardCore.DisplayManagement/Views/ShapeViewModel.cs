using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement.Views;

public class ShapeViewModel : IShape, IPositioned
{
    public ShapeViewModel()
    {
    }

    public ShapeViewModel(string shapeType)
    {
        ArgumentException.ThrowIfNullOrEmpty(shapeType);

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

    private bool _sorted;

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
            return ValueTask.FromResult<IShape>(this);
        }

        position ??= string.Empty;
        _sorted = false;
        _items ??= [];

        var wrapped = PositionWrapper.TryWrap(item, position);
        if (wrapped is not null)
        {
            _items.Add(wrapped);
        }

        return ValueTask.FromResult<IShape>(this);
    }
}

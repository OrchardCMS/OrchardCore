using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement;

public sealed class PositionWrapper : IHtmlContent, IPositioned, IShape
{
    private readonly IHtmlContent _value;
    public string Position { get; set; }

    public ShapeMetadata Metadata { get; set; } = new ShapeMetadata();

    public string Id { get; set; }

    public string TagName { get; set; }

    public IList<string> Classes { get; }

    public IDictionary<string, string> Attributes { get; }

    private Dictionary<string, object> _properties;

    public IDictionary<string, object> Properties => _properties ??= [];

    public IReadOnlyList<IPositioned> Items => throw new System.NotImplementedException();

    private PositionWrapper(IHtmlContent value, string position)
    {
        _value = value;
        Position = position;
    }

    private PositionWrapper(string value, string position)
    {
        _value = new HtmlContentString(value);
        Position = position;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        _value.WriteTo(writer, encoder);
    }

    public ValueTask<IShape> AddAsync(object item, string position)
    {
        throw new System.NotImplementedException();
    }

    public static IPositioned TryWrap(object value, string position)
    {
        if (value is IPositioned wrapper)
        {
            // Update the new Position
            if (position != null)
            {
                wrapper.Position = position;
            }
            return wrapper;
        }
        else if (value is IHtmlContent content)
        {
            return new PositionWrapper(content, position);
        }
        else if (value is string stringContent)
        {
            return new PositionWrapper(stringContent, position);
        }
        else
        {
            return null;
        }
    }

    public static IHtmlContent UnWrap(PositionWrapper wrapper)
        => wrapper._value;
}

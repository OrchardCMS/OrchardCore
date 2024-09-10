using System.Text.Encodings.Web;

namespace OrchardCore.DisplayManagement.Notify;

internal sealed class NotifyEntryComparer : IEqualityComparer<NotifyEntry>
{
    private readonly HtmlEncoder _htmlEncoder;

    public NotifyEntryComparer(HtmlEncoder htmlEncoder)
    {
        _htmlEncoder = htmlEncoder;
    }

    public bool Equals(NotifyEntry x, NotifyEntry y)
    {
        return x.Type == y.Type && x.ToHtmlString(_htmlEncoder) == y.ToHtmlString(_htmlEncoder);
    }

    public int GetHashCode(NotifyEntry obj)
    {
        return HashCode.Combine(obj.ToHtmlString(_htmlEncoder), obj.Type);
    }
}

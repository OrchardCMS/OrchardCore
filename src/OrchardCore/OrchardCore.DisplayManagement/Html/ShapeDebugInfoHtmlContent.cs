using System.Text.Encodings.Web;
using Cysharp.Text;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Html;

internal sealed class ShapeDebugInfoHtmlContent : IHtmlContent
{
    private readonly IHtmlContent _content;
    private readonly string _startComment;
    private readonly string _endComment;

    public ShapeDebugInfoHtmlContent(IHtmlContent content, string startComment, string endComment)
    {
        _content = content ?? HtmlString.Empty;
        _startComment = startComment;
        _endComment = endComment;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.Write(_startComment);
        _content.WriteTo(writer, encoder);
        writer.Write(_endComment);
    }

    public override string ToString()
    {
        using var writer = new ZStringWriter();
        WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}

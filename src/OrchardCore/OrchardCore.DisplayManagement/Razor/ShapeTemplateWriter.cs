using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Razor;

/// <summary>
/// The output of a Razor shape template executed by the <see cref="RazorShapeTemplateViewEngine"/>: the text the
/// template writes and the <see cref="IHtmlContent"/> of its child shapes, kept by reference as an MVC view buffer
/// does, so that nothing is copied until the page is written to the response. It is also an
/// <see cref="IHtmlContentBuilder"/>, so that an <see cref="IHtmlContentContainer"/> whose instance is reused, e.g.
/// the output of a tag helper, moves its content into it instead of being referenced.
/// </summary>
internal sealed class ShapeTemplateWriter : TextWriter, IHtmlContent, IHtmlContentBuilder
{
    // Strings and IHtmlContent instances, in the order they were written.
    private readonly List<object> _entries = new(8);

    public override Encoding Encoding => Encoding.UTF8;

    /// <summary>
    /// Appends a content by reference, which is written when this writer is.
    /// </summary>
    public void Append(IHtmlContent content)
    {
        if (content is not null && !ReferenceEquals(content, HtmlString.Empty))
        {
            _entries.Add(content);
        }
    }

    /// <summary>
    /// Appends a text that is HTML encoded when this writer is written.
    /// </summary>
    IHtmlContentBuilder IHtmlContentBuilder.Append(string unencoded)
    {
        if (!string.IsNullOrEmpty(unencoded))
        {
            _entries.Add(new Microsoft.AspNetCore.Mvc.ViewFeatures.StringHtmlContent(unencoded));
        }

        return this;
    }

    IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(string encoded)
    {
        Write(encoded);
        return this;
    }

    IHtmlContentBuilder IHtmlContentBuilder.AppendHtml(IHtmlContent content)
    {
        Append(content);
        return this;
    }

    IHtmlContentBuilder IHtmlContentBuilder.Clear()
    {
        _entries.Clear();
        return this;
    }

    public void CopyTo(IHtmlContentBuilder destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        for (var i = 0; i < _entries.Count; i++)
        {
            switch (_entries[i])
            {
                case string text:
                    destination.AppendHtml(text);
                    break;
                case IHtmlContentContainer container:
                    container.CopyTo(destination);
                    break;
                case IHtmlContent content:
                    destination.AppendHtml(content);
                    break;
            }
        }
    }

    public void MoveTo(IHtmlContentBuilder destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        for (var i = 0; i < _entries.Count; i++)
        {
            switch (_entries[i])
            {
                case string text:
                    destination.AppendHtml(text);
                    break;
                case IHtmlContentContainer container:
                    container.MoveTo(destination);
                    break;
                case IHtmlContent content:
                    destination.AppendHtml(content);
                    break;
            }
        }

        _entries.Clear();
    }

    public override void Write(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _entries.Add(value);
        }
    }

    public override void Write(char value) => _entries.Add(value.ToString());

    public override void Write(char[] buffer, int index, int count)
    {
        if (count > 0)
        {
            _entries.Add(new string(buffer, index, count));
        }
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        if (!buffer.IsEmpty)
        {
            _entries.Add(buffer.ToString());
        }
    }

    public override void Write(StringBuilder value)
    {
        if (value is { Length: > 0 })
        {
            _entries.Add(value.ToString());
        }
    }

    public override void WriteLine() => _entries.Add(CoreNewLine.Length == 1 ? "\n" : "\r\n");

    public override Task WriteAsync(string value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        Write(buffer.Span);
        return Task.CompletedTask;
    }

    public override Task FlushAsync() => Task.CompletedTask;

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        // Iterated by index to not allocate an enumerator.
        for (var i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];

            if (entry is string text)
            {
                writer.Write(text);
            }
            else
            {
                ((IHtmlContent)entry).WriteTo(writer, encoder);
            }
        }
    }
}

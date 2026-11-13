using System.Text.Encodings.Web;
using Fluid;
using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid;

namespace OrchardCore.Tests.DisplayManagement;

public class LiquidFluidOutputTests
{
    private static readonly FluidParser s_parser = new();

    [Fact]
    public async Task RenderAsync_TemplateWithValuesToEncode_WritesEncodedContent()
    {
        Assert.True(s_parser.TryParse("<b>{{ value }}</b>", out var template, out var error), error);

        var context = new TemplateContext();
        context.SetValue("value", "<script> & é");

        using var content = new ViewBufferTextWriterContent();

        await template.RenderAsync(content, HtmlEncoder.Default, context);

        Assert.Equal($"<b>{HtmlEncoder.Default.Encode("<script> & é")}</b>", WriteTo(content));
    }

    [Fact]
    public async Task RenderAsync_ContentLargerThanItsBuffers_KeepsTheWholeContent()
    {
        var value = new string('<', 20_000);

        Assert.True(s_parser.TryParse("{{ value }}|{{ value }}", out var template, out var error), error);

        var context = new TemplateContext();
        context.SetValue("value", value);

        using var content = new ViewBufferTextWriterContent();

        await template.RenderAsync(content, HtmlEncoder.Default, context);

        var expected = $"{HtmlEncoder.Default.Encode(value)}|{HtmlEncoder.Default.Encode(value)}";

        Assert.Equal(expected, content.ToString());
        Assert.Equal(expected, WriteTo(content));
    }

    [Fact]
    public void Advance_WithoutBuffer_Throws()
    {
        using var content = new ViewBufferTextWriterContent();

        Assert.Throws<InvalidOperationException>(() => content.Advance(1));
    }

    [Fact]
    public void Write_HtmlContentToOutputWhichIsATextWriter_DoesNotUseAnAdapter()
    {
        using var content = new ViewBufferTextWriterContent();

        Assert.Same(content, ((IFluidOutput)content).AsTextWriter());

        ((IFluidOutput)content).Write(new HtmlString("<i>"), HtmlEncoder.Default);

        Assert.Equal("<i>", content.ToString());
    }

    [Fact]
    public async Task FluidOutputTextWriter_AnyWrite_ReachesTheOutput()
    {
        using var content = new ViewBufferTextWriterContent();

        // Hides that the output is a TextWriter.
        var writer = new FluidOutputTextWriter(new ForwardingOutput(content));

        writer.Write('a');
        writer.Write("bc");
        writer.Write(['d', 'e'], 0, 2);
        writer.Write("fg".AsSpan());
        await writer.WriteAsync("hi");

        Assert.Equal("abcdefghi", content.ToString());
    }

    private static string WriteTo(ViewBufferTextWriterContent content)
    {
        using var writer = new StringWriter();

        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }

    private sealed class ForwardingOutput : IFluidOutput
    {
        private readonly IFluidOutput _output;

        public ForwardingOutput(IFluidOutput output) => _output = output;

        public void Advance(int count) => _output.Advance(count);

        public Memory<char> GetMemory(int sizeHint = 0) => _output.GetMemory(sizeHint);

        public Span<char> GetSpan(int sizeHint = 0) => _output.GetSpan(sizeHint);

        public void Write(string value) => _output.Write(value);

        public void Write(char[] buffer, int index, int count) => _output.Write(buffer, index, count);

        public ValueTask FlushAsync() => _output.FlushAsync();
    }
}

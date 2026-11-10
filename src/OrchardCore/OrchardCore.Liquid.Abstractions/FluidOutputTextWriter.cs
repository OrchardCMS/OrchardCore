using System.Text;
using Fluid;

namespace OrchardCore.Liquid;

/// <summary>
/// A <see cref="TextWriter"/> that writes to an <see cref="IFluidOutput"/>, for the components that render to a writer
/// from a Liquid tag or value, e.g. an <see cref="Microsoft.AspNetCore.Html.IHtmlContent"/>.
/// </summary>
/// <remarks>
/// Prefer <see cref="FluidOutputHtmlExtensions.AsTextWriter(IFluidOutput)"/>, which doesn't allocate this adapter when
/// the output is also a <see cref="TextWriter"/>.
/// </remarks>
public sealed class FluidOutputTextWriter : TextWriter
{
    private readonly IFluidOutput _output;

    public FluidOutputTextWriter(IFluidOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        _output = output;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        _output.GetSpan(1)[0] = value;
        _output.Advance(1);
    }

    public override void Write(string value) => _output.Write(value);

    public override void Write(char[] buffer, int index, int count) => _output.Write(buffer, index, count);

    public override void Write(ReadOnlySpan<char> buffer)
    {
        while (!buffer.IsEmpty)
        {
            var destination = _output.GetSpan(1);
            var count = Math.Min(destination.Length, buffer.Length);

            buffer[..count].CopyTo(destination);
            _output.Advance(count);

            buffer = buffer[count..];
        }
    }

    public override void Flush()
    {
        // Flushing is left to the render the output belongs to.
    }

    public override Task FlushAsync() => Task.CompletedTask;

    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(string value)
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
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Write(buffer.Span);
        return Task.CompletedTask;
    }
}

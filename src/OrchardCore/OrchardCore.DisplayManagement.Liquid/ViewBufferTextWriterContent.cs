using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using Fluid;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid;

/// <summary>
/// An <see cref="IHtmlContent"/> implementation that inherits from <see cref="TextWriter"/> to write to the ASP.NET ViewBufferTextWriter
/// in an optimal way. It is also an <see cref="IFluidOutput"/>, such that a Liquid template renders in it without another buffer.
/// </summary>
public class ViewBufferTextWriterContent : TextWriter, IHtmlContent, IFluidOutput
{
    private const int MinimumRentedBufferSize = 1024;

    private char[] _rentedBuffer;
    private StringBuilder _builder;
    private StringBuilderPool _pooledBuilder;
    private List<StringBuilderPool> _previousPooledBuilders;
    private readonly bool _releaseOnWrite;

    public override Encoding Encoding => Encoding.UTF8;

    public ViewBufferTextWriterContent(bool releaseOnWrite = true)
    {
        _pooledBuilder = StringBuilderPool.GetInstance();
        _builder = _pooledBuilder.Builder;
        _releaseOnWrite = releaseOnWrite;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleasePooledBuffer();
    }

    private void ReleasePooledBuffer()
    {
        if (_pooledBuilder != null)
        {
            _pooledBuilder.Dispose();
            _pooledBuilder = null;
            _builder = null;

            if (_previousPooledBuilders != null)
            {
                foreach (var pooledBuilder in _previousPooledBuilders)
                {
                    pooledBuilder.Dispose();
                }

                _previousPooledBuilders = null;
            }
        }
    }

    private StringBuilder AllocateBuilder()
    {
        _previousPooledBuilders ??= [];
        _previousPooledBuilders.Add(_pooledBuilder);

        _pooledBuilder = StringBuilderPool.GetInstance();
        _builder = _pooledBuilder.Builder;
        return _builder;
    }

    // Invoked when used as TextWriter to intercept what is supposed to be written
    public override void Write(string value)
    {
        if (value == null || value.Length == 0)
        {
            return;
        }

        if (_builder.Length + value.Length <= _builder.Capacity)
        {
            _builder.Append(value);
        }
        else
        {
            // The string doesn't fit in the buffer, rent more
            var index = 0;
            do
            {
                var sizeToCopy = Math.Min(_builder.Capacity - _builder.Length, value.Length - index);
                _builder.Append(value.AsSpan(index, sizeToCopy));

                if (_builder.Length == _builder.Capacity)
                {
                    AllocateBuilder();
                }

                index += sizeToCopy;
            } while (index < value.Length);
        }
    }

    public override void Write(char value)
    {
        if (_builder.Length >= _builder.Capacity)
        {
            AllocateBuilder();
        }

        _builder.Append(value);
    }

    public override void Write(char[] buffer)
    {
        if (buffer == null || buffer.Length == 0)
        {
            return;
        }

        if (_builder.Length + buffer.Length <= _builder.Capacity)
        {
            _builder.Append(buffer);
        }
        else
        {
            // The string doesn't fit in the buffer, rent more
            var index = 0;
            do
            {
                var sizeToCopy = Math.Min(_builder.Capacity - _builder.Length, buffer.Length - index);
                _builder.Append(buffer.AsSpan(index, sizeToCopy));

                if (_builder.Length == _builder.Capacity)
                {
                    AllocateBuilder();
                }

                index += sizeToCopy;
            } while (index < buffer.Length);
        }
    }

    public override void Write(char[] buffer, int offset, int count)
    {
        if (buffer == null || buffer.Length == 0 || count == 0)
        {
            return;
        }

        if (_builder.Length + count <= _builder.Capacity)
        {
            _builder.Append(buffer, offset, count);
        }
        else
        {
            // The string doesn't fit in the buffer, rent more
            var index = 0;
            do
            {
                var sizeToCopy = Math.Min(_builder.Capacity - _builder.Length, count - index);
                _builder.Append(buffer.AsSpan(index + offset, sizeToCopy));

                if (_builder.Length == _builder.Capacity)
                {
                    AllocateBuilder();
                }

                index += sizeToCopy;
            } while (index < count);
        }
    }

    public override void Write(ReadOnlySpan<char> buffer)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        if (_builder.Length + buffer.Length <= _builder.Capacity)
        {
            _builder.Append(buffer);
        }
        else
        {
            // The string doesn't fit in the buffer, rent more
            var index = 0;
            do
            {
                var sizeToCopy = Math.Min(_builder.Capacity - _builder.Length, buffer.Length - index);
                _builder.Append(buffer.Slice(index, sizeToCopy));

                if (_builder.Length == _builder.Capacity)
                {
                    AllocateBuilder();
                }

                index += sizeToCopy;
            } while (index < buffer.Length);
        }
    }

    public override void Write(StringBuilder value)
    {
        if (value != null)
        {
            foreach (var chunk in value.GetChunks())
            {
                if (!chunk.IsEmpty)
                {
                    Write(chunk.Span);
                }
            }
        }
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        if (_builder == null)
        {
            throw new InvalidOperationException("Buffer has already been rendered");
        }

        if (_previousPooledBuilders != null)
        {
            foreach (var pooledBuilder in _previousPooledBuilders)
            {
                foreach (var chunk in pooledBuilder.Builder.GetChunks())
                {
                    if (!chunk.IsEmpty)
                    {
                        writer.Write(chunk.Span);
                    }
                }
            }
        }

        foreach (var chunk in _builder.GetChunks())
        {
            if (!chunk.IsEmpty)
            {
                writer.Write(chunk.Span);
            }
        }

        if (_releaseOnWrite)
        {
            ReleasePooledBuffer();
        }
    }

    public override string ToString()
    {
        if (_builder == null)
        {
            throw new InvalidOperationException("Buffer has already been rendered");
        }

        if (_previousPooledBuilders == null)
        {
            return _builder.ToString();
        }

        var result = new StringBuilder();

        foreach (var pooledBuilder in _previousPooledBuilders)
        {
            result.Append(pooledBuilder.Builder);
        }

        return result.Append(_builder).ToString();
    }

    public override Task FlushAsync()
    {
        // Override since the base implementation does unnecessary work
        return Task.CompletedTask;
    }

    #region IFluidOutput

    // A string builder can't expose its buffer, which is only needed to encode a value that requires it.
    // The rented buffer is returned right away since many instances can be alive while a page is rendered.
    public Span<char> GetSpan(int sizeHint = 0) => RentBuffer(sizeHint);

    public Memory<char> GetMemory(int sizeHint = 0) => RentBuffer(sizeHint);

    public void Advance(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var buffer = _rentedBuffer ?? throw new InvalidOperationException($"'{nameof(GetSpan)}' or '{nameof(GetMemory)}' must be called first.");

        _rentedBuffer = null;

        Write(buffer.AsSpan(0, count));

        ArrayPool<char>.Shared.Return(buffer);
    }

    ValueTask IFluidOutput.FlushAsync() => ValueTask.CompletedTask;

    private char[] RentBuffer(int sizeHint)
    {
        if (_rentedBuffer == null || _rentedBuffer.Length < sizeHint)
        {
            if (_rentedBuffer != null)
            {
                ArrayPool<char>.Shared.Return(_rentedBuffer);
            }

            _rentedBuffer = ArrayPool<char>.Shared.Rent(Math.Max(sizeHint, MinimumRentedBufferSize));
        }

        return _rentedBuffer;
    }

    #endregion

    #region Async Methods

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
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Write(buffer.Span);
        return Task.CompletedTask;
    }

    public override Task WriteAsync(StringBuilder value, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        Write(value);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(char value)
    {
        WriteLine(value);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(string value)
    {
        WriteLine(value);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        WriteLine(buffer, index, count);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        WriteLine(buffer);
        return Task.CompletedTask;
    }

    public override Task WriteLineAsync(StringBuilder value, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        WriteLine(value);
        return Task.CompletedTask;
    }

    #endregion
}

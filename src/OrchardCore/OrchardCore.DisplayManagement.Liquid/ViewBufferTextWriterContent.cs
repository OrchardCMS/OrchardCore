using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// An <see cref="IHtmlContent"/> implementation that inherits from <see cref="TextWriter"/> to write to the ASP.NET ViewBufferTextWriter
    /// in an optimal way.
    /// </summary>
    public class ViewBufferTextWriterContent : TextWriter
    {
        private StringBuilder _builder;
        private StringBuilderPool _pooledBuilder;
        private List<StringBuilderPool> _previousPooledBuilders;
        private bool _disposed;

        public override Encoding Encoding => Encoding.UTF8;

        public ViewBufferTextWriterContent()
        {
            _pooledBuilder = StringBuilderPool.GetInstance();
            _builder = _pooledBuilder.Builder;
        }

        protected override void Dispose(bool disposing)
        {
            ReleasePooledBuffer();
            _disposed = true;
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

                    _previousPooledBuilders.Clear();
                    _previousPooledBuilders = null;
                }
            }
        }

        private StringBuilder AllocateBuilder()
        {
            _previousPooledBuilders ??= new List<StringBuilderPool>();
            _previousPooledBuilders.Add(_pooledBuilder);

            _pooledBuilder = StringBuilderPool.GetInstance();
            _builder = _pooledBuilder.Builder;
            return _builder;
        }

        // Invoked when used as TextWriter to intercept what is supposed to be written
        public override void Write(string value)
        {
            CheckDisposed();

            if (String.IsNullOrEmpty(value))
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
            CheckDisposed();

            if (_builder.Length >= _builder.Capacity)
            {
                AllocateBuilder();
            }

            _builder.Append(value);
        }

        public override void Write(char[] buffer)
        {
            CheckDisposed();

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
            CheckDisposed();

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
            CheckDisposed();

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

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            CheckDisposed();

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

            Dispose();
            _disposed = true;
        }

        public override Task FlushAsync()
        {
            CheckDisposed();
            // Override since the base implementation does unnecessary work
            return Task.CompletedTask;
        }

        public override Task WriteAsync(string value)
        {
            CheckDisposed();
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char value)
        {
            CheckDisposed();
            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            CheckDisposed();
            Write(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Write(buffer.Span);
            return Task.CompletedTask;
        }

        public override Task WriteAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Write(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char value)
        {
            if (_disposed)
            {
                ThrowDisposed();
            }

            WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(string value)
        {
            CheckDisposed();
            WriteLine(value);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            CheckDisposed();
            WriteLine(buffer, index, count);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            WriteLine(buffer);
            return Task.CompletedTask;
        }

        public override Task WriteLineAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            WriteLine(value);
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            using var pool = StringBuilderPool.GetInstance();
            using var sw = new StringWriter(pool.Builder);
            WriteTo(sw, NullHtmlEncoder.Default);
            return sw.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (_disposed)
            {
                ThrowDisposed();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDisposed()
        {
            throw new ObjectDisposedException("This instance has been disposed an cannot be used");
        }
    }
}

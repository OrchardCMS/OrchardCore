using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Text;

namespace OrchardCore.Abstractions.Pooling
{
    /// <summary>
    /// Based StringWriter but made to use ZString as backing implementation.
    /// </summary>
    internal sealed class ZStringWriter : TextWriter
    {
        private Utf16ValueStringBuilder _sb;
        private bool _isOpen;
        private UnicodeEncoding _encoding;

        public ZStringWriter() : this(CultureInfo.CurrentCulture)
        {
        }

        public ZStringWriter(IFormatProvider formatProvider) : base(formatProvider)
        {
            _sb = ZString.CreateStringBuilder();
            _isOpen = true;
        }

        public override void Close()
        {
            Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            _sb.Dispose();
            _isOpen = false;
            base.Dispose(disposing);
        }

        public override Encoding Encoding => _encoding ??= new UnicodeEncoding(false, false);

        // Writes a character to the underlying string buffer.
        //
        public override void Write(char value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("Buffer overflow.");
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(buffer.AsSpan(index, count));
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(buffer);
        }

        // Writes a string to the underlying string buffer. If the given string is
        // null, nothing is written.
        //
        public override void Write(string value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            if (value != null)
            {
                _sb.Append(value);
            }
        }

        public override void Write(StringBuilder value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(value);
        }

        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(buffer);
            WriteLine();
        }

        public override void WriteLine(StringBuilder value)
        {
            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(value);
            WriteLine();
        }

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

        public override Task WriteAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(value);
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

        public override Task WriteLineAsync(StringBuilder value, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            if (!_isOpen)
            {
                throw new ObjectDisposedException(nameof(_sb));
            }

            _sb.Append(value);
            WriteLine();
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

            WriteLine(buffer.Span);
            return Task.CompletedTask;
        }

        public override Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}

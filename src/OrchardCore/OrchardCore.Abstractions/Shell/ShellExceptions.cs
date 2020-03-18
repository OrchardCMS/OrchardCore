using System;

namespace OrchardCore.Environment.Shell
{
    public static class ShellExceptions
    {
        public class ConcurrencyException : Exception
        {
            /// <summary>
            /// A concurrency exception thrown by <see cref="IShellHost"/>.
            /// </summary>
            public ConcurrencyException(string message) : this(message, exception: null)
            {
            }

            /// <summary>
            /// A concurrency exception thrown by <see cref="IShellHost"/>.
            /// </summary>
            public ConcurrencyException(string message, Exception exception) : base(message, exception)
            {
            }
        }
    }
}

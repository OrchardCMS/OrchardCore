using System;

namespace OrchardCore.Environment.Shell
{
    public class ShellConcurrencyException : Exception
    {
        /// <summary>
        /// A concurrency exception thrown by <see cref="IShellHost"/>.
        /// </summary>
        public ShellConcurrencyException(string message) : this(message, exception: null)
        {
        }

        /// <summary>
        /// A concurrency exception thrown by <see cref="IShellHost"/>.
        /// </summary>
        public ShellConcurrencyException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}

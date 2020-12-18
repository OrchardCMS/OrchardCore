using System;

namespace OrchardCore.Environment.Shell
{
    /// <summary>
    /// The <see cref="Exception"/> that is thrown if <see cref="IShellHost.ReloadShellContextAsync(ShellSettings, bool)"/> fails.
    /// </summary>
    public class ShellHostReloadException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ShellHostReloadException"/> with the specified
        /// exception message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ShellHostReloadException(string message) : base(message)
        {
        }
    }
}

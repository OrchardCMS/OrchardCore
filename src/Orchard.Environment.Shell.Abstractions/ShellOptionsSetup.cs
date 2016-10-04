using Microsoft.Extensions.Options;

namespace Orchard.Environment.Shell
{
    /// <summary>
    /// Sets up default options for <see cref="ShellOptions"/>.
    /// </summary>
    public class ShellOptionsSetup : ConfigureOptions<ShellOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShellOptions"/>.
        /// </summary>
        public ShellOptionsSetup()
            : base(options => { })
        {
        }
    }
}
using Microsoft.Extensions.OptionsModel;

namespace Orchard.Environment.Shell.Descriptor.Settings
{
    /// <summary>
    /// Sets up default options for <see cref="ShellDescriptorOptions"/>.
    /// </summary>
    public class ShellDescriptorOptionsSetup : ConfigureOptions<ShellDescriptorOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShellDescriptorOptions"/>.
        /// </summary>
        public ShellDescriptorOptionsSetup()
            : base(options => { })
        {
        }
    }
}
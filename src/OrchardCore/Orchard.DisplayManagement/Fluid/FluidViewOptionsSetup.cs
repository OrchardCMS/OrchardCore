using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Orchard.DisplayManagement.Fluid
{
    /// <summary>
    /// Sets up default options for <see cref="FluidViewOptions"/>.
    /// </summary>
    public class FluidViewOptionsSetup : ConfigureOptions<FluidViewOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FluidViewOptions"/>.
        /// </summary>
        /// <param name="hostingEnvironment"><see cref="IHostingEnvironment"/> for the application.</param>
        public FluidViewOptionsSetup(IHostingEnvironment hostingEnvironment)
            : base(options => ConfigureFluid(options, hostingEnvironment))
        {
        }

        private static void ConfigureFluid(FluidViewOptions options, IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment.ContentRootFileProvider != null)
            {
                options.FileProviders.Add(hostingEnvironment.ContentRootFileProvider);
            }
        }
    }
}
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewOptionsSetup : IConfigureOptions<FluidViewOptions>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of <see cref="FluidViewOptions"/>.
        /// </summary>
        /// <param name="hostingEnvironment"><see cref="IHostingEnvironment"/> for the application.</param>
        public FluidViewOptionsSetup(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public void Configure(FluidViewOptions options)
        {
            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                options.FileProviders.Add(_hostingEnvironment.ContentRootFileProvider);
            }
        }
    }
}
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Mvc;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewOptionsSetup : IConfigureOptions<LiquidViewOptions>
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IApplicationContext _applicationContext;

        /// <summary>
        /// Initializes a new instance of <see cref="LiquidViewOptions"/>.
        /// </summary>
        /// <param name="hostingEnvironment"><see cref="IHostEnvironment"/> for the application.</param>
        /// <param name="applicationContext"><see cref="IApplicationContext"/> for the application.</param>
        public LiquidViewOptionsSetup(IHostEnvironment hostingEnvironment, IApplicationContext applicationContext)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _applicationContext = applicationContext ?? throw new ArgumentNullException(nameof(applicationContext));
        }

        public void Configure(LiquidViewOptions options)
        {
            if (_hostingEnvironment.ContentRootFileProvider != null)
            {
                options.FileProviders.Add(_hostingEnvironment.ContentRootFileProvider);

                // To let the application behave as a module, its views are requested under the virtual
                // "Areas" folder, but they are still served from the file system by this custom provider.
                options.FileProviders.Insert(0, new ApplicationViewFileProvider(_applicationContext));

                if (_hostingEnvironment.IsDevelopment())
                {
                    // While in development, liquid files are 1st served from their module project locations.
                    options.FileProviders.Insert(0, new ModuleProjectLiquidFileProvider(_applicationContext));
                }
            }
        }
    }
}

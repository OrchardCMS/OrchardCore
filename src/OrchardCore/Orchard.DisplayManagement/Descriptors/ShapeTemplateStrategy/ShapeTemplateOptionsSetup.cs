using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy
{
    /// <summary>
    /// Sets up default options for <see cref="ShapeTemplateViewOptions"/>.
    /// </summary>
    public class ShapeTemplateOptionsSetup : ConfigureOptions<ShapeTemplateOptions>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ShapeTemplateViewOptions"/>.
        /// </summary>
        /// <param name="hostingEnvironment"><see cref="IHostingEnvironment"/> for the application.</param>
        public ShapeTemplateOptionsSetup(IHostingEnvironment hostingEnvironment)
            : base(options => ConfigureShapeTemplate(options, hostingEnvironment))
        {
        }

        private static void ConfigureShapeTemplate(ShapeTemplateOptions options, IHostingEnvironment hostingEnvironment)
        {
            if (hostingEnvironment.ContentRootFileProvider != null)
            {
                options.FileProviders.Add(hostingEnvironment.ContentRootFileProvider);
            }
        }
    }
}
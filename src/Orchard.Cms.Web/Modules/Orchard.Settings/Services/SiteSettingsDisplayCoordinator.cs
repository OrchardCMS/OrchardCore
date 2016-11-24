using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Coordinates all <see cref="ISiteSettingsDisplayDriver"/> implementations to 
    /// render all the provided shapes in their expected group and location.
    /// </summary>
    public class SiteSettingsDisplayCoordinator : ISiteSettingsDisplayHandler
    {
        private readonly IEnumerable<ISiteSettingsDisplayDriver> _drivers;
        
        public SiteSettingsDisplayCoordinator(
            IEnumerable<ISiteSettingsDisplayDriver> drivers,
            ILogger<SiteSettingsDisplayCoordinator> logger)
        {
            _drivers = drivers;

            Logger = logger;
        }

        ILogger Logger { get; }

        public async Task BuildEditorAsync(ISite site, BuildEditorContext context)
        {
            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.BuildEditorAsync(site, context);
                if (result != null)
                {
                    result.Apply(context);
                }
            }, Logger);
        }

        public async Task UpdateEditorAsync(ISite site, UpdateEditorContext context)
        {
            await _drivers.InvokeAsync(async driver =>
            {
                var result = await driver.UpdateEditorAsync(site, context);
                if (result != null)
                {
                    result.Apply(context);
                }
            }, Logger);
        }
    }
}

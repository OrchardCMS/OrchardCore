using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Logging;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Layout;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Theming;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// Processes all <see cref="ISiteSettingsDisplayHandler"/> implementations to provide the 
    /// edit and update shapes for the Site Settings object.
    /// </summary>
    public class SiteSettingsDisplayManager : BaseDisplayManager, ISiteSettingsDisplayManager
    {
        private readonly IEnumerable<ISiteSettingsDisplayHandler> _handlers;
        private readonly IShapeTableManager _shapeTableManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IThemeManager _themeManager;
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly ISiteService _siteService;

        public SiteSettingsDisplayManager(
            IEnumerable<ISiteSettingsDisplayHandler> handlers,
            IShapeTableManager shapeTableManager,
            IShapeFactory shapeFactory,
            IThemeManager themeManager,
            ILogger<SiteSettingsDisplayManager> logger,
            ILayoutAccessor layoutAccessor,
            ISiteService siteService
            ) : base(shapeTableManager, shapeFactory, themeManager)
        {
            _handlers = handlers;
            _shapeTableManager = shapeTableManager;
            _shapeFactory = shapeFactory;
            _themeManager = themeManager;
            _layoutAccessor = layoutAccessor;
            _siteService = siteService;

            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<dynamic> BuildEditorAsync(IUpdateModel updater, string groupId)
        {
            var actualShapeType = "Settings" + "_Edit";
            var site = await _siteService.GetSiteSettingsAsync();

            dynamic itemShape = CreateContentShape(actualShapeType);

            var context = new BuildEditorContext(
                itemShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync(handler => handler.BuildEditorAsync(site, context), Logger);

            return context.Shape;
        }

        public async Task<dynamic> UpdateEditorAsync(IUpdateModel updater, string groupId)
        {
            var actualShapeType = "Settings" + "_Edit";
            var site = await _siteService.GetSiteSettingsAsync();

            dynamic itemShape = CreateContentShape(actualShapeType);

            var context = new UpdateEditorContext(
                itemShape,
                groupId,
                "",
                _shapeFactory,
                _layoutAccessor.GetLayout(),
                updater
            );

            await BindPlacementAsync(context);

            await _handlers.InvokeAsync(handler => handler.UpdateEditorAsync(site, context), Logger);

            return context.Shape;
        }
    }
}

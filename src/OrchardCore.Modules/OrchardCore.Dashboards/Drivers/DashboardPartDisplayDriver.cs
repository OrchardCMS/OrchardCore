using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Dashboards.Models;
using OrchardCore.Dashboards.Services;
using OrchardCore.Dashboards.ViewModels;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Dashboards.Drivers;

internal sealed class DashboardPartDisplayDriver : ContentPartDisplayDriver<DashboardPart>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDashboardManager _dashboardManager;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;
    private readonly DashboardOptions _options;

    internal readonly IStringLocalizer S;

    public DashboardPartDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IDashboardManager dashboardManager,
        IStringLocalizerFactory stringLocalizerFactory,
        IOptions<DashboardOptions> options,
        IStringLocalizer<DashboardPartDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _dashboardManager = dashboardManager;
        _stringLocalizerFactory = stringLocalizerFactory;
        _options = options.Value;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(DashboardPart part, BuildPartDisplayContext context)
    {
        return Task.FromResult<IDisplayResult>(null);
    }

    public override IDisplayResult Edit(DashboardPart dashboardPart, BuildPartEditorContext context)
    {
        return Initialize<DashboardPartViewModel>(GetEditorShapeType(context), model =>
        {
            model.Position = dashboardPart.Position;
            model.Width = dashboardPart.Width;
            model.Height = dashboardPart.Height;
            model.DashboardPart = dashboardPart;
            model.ContentItem = dashboardPart.ContentItem;
            model.IsNew = context.IsNew;
            model.DashboardType = dashboardPart.DashboardType;

            if (context.IsNew && string.IsNullOrEmpty(model.DashboardType))
            {
                var requestedType = _httpContextAccessor.HttpContext?.Request.Query[DashboardConstants.DashboardTypeQueryName].ToString();

                model.DashboardType = string.IsNullOrEmpty(requestedType) ? _options.DefaultType : requestedType;
            }

            var type = _dashboardManager.GetType(model.DashboardType);

            if (type is not null)
            {
                model.DashboardType = type.Name;
                model.DashboardTypeDisplayName = _stringLocalizerFactory.Localize(type.DisplayName);
            }
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(DashboardPart part, UpdatePartEditorContext context)
    {
        var model = new DashboardPartViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Position,
            m => m.Width,
            m => m.Height,
            m => m.DashboardType);

        part.Position = model.Position;
        part.Width = model.Width;
        part.Height = model.Height;

        // The dashboard of a widget is set when it is created, and never changed from the editor afterwards.
        if (context.IsNew)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var type = _dashboardManager.GetType(string.IsNullOrEmpty(model.DashboardType) ? _options.DefaultType : model.DashboardType);

            if (type is null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DashboardType), S["The dashboard of the widget is unknown."]);
            }
            else if (!await _dashboardManager.CanManageAsync(user, type))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DashboardType), S["You are not allowed to add widgets to this dashboard."]);
            }
            else
            {
                part.DashboardType = type.Name;
                part.OwnerId = type.Scope == DashboardScope.User ? DashboardManager.GetUserId(user) : null;
            }
        }

        return Edit(part, context);
    }
}

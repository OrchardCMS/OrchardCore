using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Liquid.Fields;

namespace OrchardCore.Liquid.Handlers;

public sealed class LiquidFieldHandler : ContentFieldHandler<LiquidField>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public LiquidFieldHandler(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<LiquidFieldHandler> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
        S = localizer;
    }

    public override async Task ValidatingAsync(ValidateContentFieldContext context, LiquidField field)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null &&
            !await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageLiquidTemplates))
        {
            context.Fail(S["You do not have permission to manage Liquid templates."], nameof(field.Liquid));
        }

        if (!string.IsNullOrEmpty(field.Liquid) &&
            !_liquidTemplateManager.Validate(field.Liquid, out var errors))
        {
            context.Fail(
                S["The Liquid field doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)],
                nameof(field.Liquid));
        }
    }
}

using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.ViewModels;

namespace OrchardCore.Liquid.Handlers;

public class LiquidPartHandler : ContentPartHandler<LiquidPart>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly HtmlEncoder _htmlEncoder;

    internal readonly IStringLocalizer S;

    public LiquidPartHandler(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<LiquidPartHandler> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
        _htmlEncoder = htmlEncoder;
        S = localizer;
    }

    protected override async Task ValidatingAsync(ValidateContentPartContext context, LiquidPart part)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null &&
            !await _authorizationService.AuthorizeAsync(httpContext.User, Permissions.ManageLiquidTemplates))
        {
            context.Fail(S["You do not have permission to manage Liquid templates."], nameof(part.Liquid));
        }

        if (!string.IsNullOrEmpty(part.Liquid) &&
            !_liquidTemplateManager.Validate(part.Liquid, out var errors))
        {
            context.Fail(
                S["The Liquid Body doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)],
                nameof(part.Liquid));
        }
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LiquidPart part)
    {
        return context.ForAsync<BodyAspect>(async bodyAspect =>
        {
            try
            {
                var model = new LiquidPartViewModel()
                {
                    LiquidPart = part,
                    ContentItem = part.ContentItem,
                };

                var result = await _liquidTemplateManager.RenderHtmlContentAsync(part.Liquid, _htmlEncoder, model,
                    new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });

                bodyAspect.Body = result;
            }
            catch
            {
                bodyAspect.Body = HtmlString.Empty;
            }
        });
    }
}

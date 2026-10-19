using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.DisplayManagement;

/// <inheritdoc />
public sealed class ShapeHtmlRenderer : IShapeHtmlRenderer
{
    private readonly IDisplayHelper _displayHelper;
    private readonly ViewContextAccessor _viewContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly HtmlEncoder _htmlEncoder;

    public ShapeHtmlRenderer(
        IDisplayHelper displayHelper,
        ViewContextAccessor viewContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        ITempDataProvider tempDataProvider,
        HtmlEncoder htmlEncoder)
    {
        _displayHelper = displayHelper;
        _viewContextAccessor = viewContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _tempDataProvider = tempDataProvider;
        _htmlEncoder = htmlEncoder;
    }

    /// <inheritdoc />
    public Task<string> RenderAsync(IShape shape, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shape);

        return RenderOutsideViewAsync(shape, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string> RenderZoneAsync(IShape shape, string zoneName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(shape);
        ArgumentException.ThrowIfNullOrEmpty(zoneName);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<string>(cancellationToken);
        }

        if (shape is not IZoneHolding zoneHolding || !zoneHolding.Zones.IsNotEmpty(zoneName))
        {
            return Task.FromResult(string.Empty);
        }

        return RenderOutsideViewAsync(zoneHolding.Zones[zoneName], cancellationToken);
    }

    // The display pipeline can't be stopped once it runs, so the token is checked before rendering starts.
    private async Task<string> RenderOutsideViewAsync(IShape shape, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var previousViewContext = _viewContextAccessor.ViewContext;

        if (previousViewContext?.View is null && _httpContextAccessor.HttpContext is { } httpContext)
        {
            var actionContext = await httpContext.GetActionContextAsync();

            _viewContextAccessor.ViewContext = new ViewContext(
                actionContext,
                NullView.Instance,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                new TempDataDictionary(httpContext, _tempDataProvider),
                TextWriter.Null,
                new HtmlHelperOptions());
        }

        try
        {
            var content = await _displayHelper.ShapeExecuteAsync(shape);

            using var writer = new StringWriter();
            content.WriteTo(writer, _htmlEncoder);

            return writer.ToString();
        }
        finally
        {
            _viewContextAccessor.ViewContext = previousViewContext;
        }
    }

    private sealed class NullView : IView
    {
        public static readonly NullView Instance = new();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            return Task.CompletedTask;
        }
    }
}

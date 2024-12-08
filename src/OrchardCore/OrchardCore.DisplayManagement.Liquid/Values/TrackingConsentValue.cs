using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;

#nullable enable

namespace OrchardCore.DisplayManagement.Liquid.Values;

/// <summary>
/// Exposes <see href="ITrackingConsentFeature" />properties to Liquid.
/// </summary>
/// <remarks>
/// Exposes the following properties:
/// - CanTrack
/// - HasConsent
/// - IsConsentNeeded
/// - CookieName
/// - CookieValue
/// </remarks>
internal sealed class TrackingConsentValue : FluidValue
{
    public override FluidValues Type => FluidValues.Object;

    public override bool Equals(FluidValue other) 
        => other is TrackingConsentValue;

    public override bool ToBooleanValue() => true;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => null!;

    public override string ToStringValue() => "TrackingConsent";

    protected override FluidValue GetValue(string name, TemplateContext context)
    {
        var feature = GetTrackingFeature(context);

        if (feature is null)
        {
            return NilValue.Instance;
        }

        return name switch
        {
            nameof(ITrackingConsentFeature.CanTrack) => BooleanValue.Create(feature.CanTrack),
            nameof(ITrackingConsentFeature.HasConsent) => BooleanValue.Create(feature.HasConsent),
            nameof(ITrackingConsentFeature.IsConsentNeeded) => BooleanValue.Create(feature.IsConsentNeeded),
            "CookieName" => new StringValue(GetCookiePolicyOptions(context)?.ConsentCookie?.Name ?? string.Empty),
            "CookieValue" => new StringValue(GetCookiePolicyOptions(context)?.ConsentCookieValue ?? string.Empty),
            _ => NilValue.Instance
        };
    }

    private static ITrackingConsentFeature? GetTrackingFeature(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var httpContext = ctx.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;

        return httpContext?.Features.Get<ITrackingConsentFeature>();
    }

    private static CookiePolicyOptions? GetCookiePolicyOptions(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        return ctx.Services.GetService<IOptions<CookiePolicyOptions>>()?.Value;
    }
}

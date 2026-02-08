using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MediaSizeLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
{
    public int Order { get; set; } = 900;

    /// <inheritdoc />
    public bool IsReusable => true;

    /// <inheritdoc />
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetRequiredService<IEnumerable<IMediaSizeLimitProvider>>();

        return new InternalMediaSizeFilter(providers);
    }

    private sealed class InternalMediaSizeFilter : IAsyncAuthorizationFilter, IRequestFormLimitsPolicy
    {
        private readonly IEnumerable<IMediaSizeLimitProvider> _providers;
        public InternalMediaSizeFilter(IEnumerable<IMediaSizeLimitProvider> providers)
        {
            _providers = providers;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var maxFileSize = 0L;
            foreach (var provider in _providers.OrderBy(provider => provider.Order))
            {
                if (await provider.GetMediaSizeLimitAsync() is { } max)
                {
                    maxFileSize = max;
                    break;
                }
            }

            var effectiveFormPolicy = context.FindEffectivePolicy<IRequestFormLimitsPolicy>();
            if (effectiveFormPolicy == null || effectiveFormPolicy == this)
            {
                var features = context.HttpContext.Features;
                var formFeature = features.Get<IFormFeature>();

                if (formFeature == null || formFeature.Form == null)
                {
                    // Request form has not been read yet, so set the limits
                    var formOptions = new FormOptions
                    {
                        MultipartBodyLengthLimit = maxFileSize,
                    };

                    features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));
                }
            }

            var effectiveRequestSizePolicy = context.FindEffectivePolicy<IRequestSizePolicy>();
            if (effectiveRequestSizePolicy == null)
            {
                // Will only be available when running OutOfProcess with Kestrel.
                var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

                if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
                {
                    maxRequestBodySizeFeature.MaxRequestBodySize = maxFileSize;
                }
            }
        }
    }
}

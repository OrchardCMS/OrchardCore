using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Media.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MediaSizeLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public int Order { get; set; } = 900;

        /// <inheritdoc />
        public bool IsReusable => true;

        /// <inheritdoc />
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<MediaOptions>>();

            return new InternalMediaSizeFilter(options.Value.MaxFileSize);
        }

        private class InternalMediaSizeFilter : IAuthorizationFilter, IRequestFormLimitsPolicy
        {
            private readonly long _maxFileSize;

            public InternalMediaSizeFilter(long maxFileSize)
            {
                _maxFileSize = maxFileSize;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
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
                            MultipartBodyLengthLimit = _maxFileSize
                        };

                        features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, formOptions));
                    }
                }

                var effectiveRequestSizePolicy = context.FindEffectivePolicy<IRequestSizePolicy>();
                if (effectiveRequestSizePolicy == null || effectiveRequestSizePolicy == this)
                {
                    //  Will only be available when running OutOfProcess with Kestrel
                    var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

                    if (maxRequestBodySizeFeature != null && !maxRequestBodySizeFeature.IsReadOnly)
                    {
                        maxRequestBodySizeFeature.MaxRequestBodySize = _maxFileSize;
                    }
                }
            }
        }
    }
}

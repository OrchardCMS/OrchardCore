using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;

namespace OrchardCore.Liquid.Services
{
    public class LiquidTemplateManager : ILiquidTemplateManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly LiquidViewParser _liquidViewParser;
        private readonly TemplateOptions _templateOptions;
        private readonly IServiceProvider _serviceProvider;

        public LiquidTemplateManager(
            IMemoryCache memoryCache,
            LiquidViewParser liquidViewParser,
            IOptions<TemplateOptions> templateOptions,
            IServiceProvider serviceProvider
            )
        {
            _memoryCache = memoryCache;
            _liquidViewParser = liquidViewParser;
            _templateOptions = templateOptions.Value;
            _serviceProvider = serviceProvider;
        }

        public Task<string> RenderStringAsync(string source, TextEncoder encoder, object model = null, IEnumerable<KeyValuePair<string, FluidValue>> properties = null)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return Task.FromResult((string)null);
            }

            var result = GetCachedTemplate(source);
            var context = new LiquidTemplateContext(_serviceProvider, _templateOptions);

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    context.SetValue(property.Key, property.Value);
                }
            }

            return result.RenderAsync(encoder, context, model);
        }

        public async Task<IHtmlContent> RenderHtmlContentAsync(string source, TextEncoder encoder, object model = null, IEnumerable<KeyValuePair<string, FluidValue>> properties = null)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return HtmlString.Empty;
            }

            var result = GetCachedTemplate(source);
            var context = new LiquidTemplateContext(_serviceProvider, _templateOptions);

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    context.SetValue(property.Key, property.Value);
                }
            }

            var htmlContentWriter = new ViewBufferTextWriterContent();

            await result.RenderAsync(htmlContentWriter, encoder, context, model);

            return htmlContentWriter;
        }

        public Task RenderAsync(string source, TextWriter writer, TextEncoder encoder, object model = null, IEnumerable<KeyValuePair<string, FluidValue>> properties = null)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return Task.CompletedTask;
            }

            var result = GetCachedTemplate(source);
            var context = new LiquidTemplateContext(_serviceProvider, _templateOptions);

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    context.SetValue(property.Key, property.Value);
                }
            }

            return result.RenderAsync(writer, encoder, context, model);
        }

        public LiquidViewTemplate GetCachedTemplate(string source)
        {
            var errors = Enumerable.Empty<string>();

            var result = _memoryCache.GetOrCreate(source, (ICacheEntry e) =>
            {
                if (!_liquidViewParser.TryParse(source, out var parsed, out var error))
                {
                    // If the source string cannot be parsed, create a template that contains the parser errors
                    _liquidViewParser.TryParse(String.Join(System.Environment.NewLine, errors), out parsed, out error);
                }

                // Define a default sliding expiration to prevent the
                // cache from being filled and still apply some micro-caching
                // in case the template is used commonly
                e.SetSlidingExpiration(TimeSpan.FromSeconds(30));
                return new LiquidViewTemplate(parsed);
            });

            return result;
        }

        public bool Validate(string template, out IEnumerable<string> errors)
        {
            var success = _liquidViewParser.TryParse(template, out _, out var error);
            errors = new[] { error };
            return success;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Liquid.Services
{
    public class LiquidTemplateManager : ILiquidTemplateManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly LiquidViewParser _liquidViewParser;
        private readonly TemplateOptions _templateOptions;

        public LiquidTemplateManager(IMemoryCache memoryCache, LiquidViewParser liquidViewParser, IOptions<TemplateOptions> templateOptions)
        {
            _memoryCache = memoryCache;
            _liquidViewParser = liquidViewParser;
            _templateOptions = templateOptions.Value;
        }

        public Task<string> RenderAsync(string source, TextEncoder encoder, object model, Action<LiquidTemplateContext> action)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return Task.FromResult((string)null);
            }

            var result = GetCachedTemplate(source);
            var context = new LiquidTemplateContext(ShellScope.Services, _templateOptions);

            return result.RenderAsync(encoder, context, model, action);
        }

        public Task RenderAsync(string source, TextWriter writer, TextEncoder encoder, object model, Action<LiquidTemplateContext> action)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                return Task.CompletedTask;
            }

            var result = GetCachedTemplate(source);
            var context = new LiquidTemplateContext(ShellScope.Services, _templateOptions);

            return result.RenderAsync(writer, encoder, context, model, action);
        }

        private LiquidViewTemplate GetCachedTemplate(string source)
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
            errors = new [] { error };
            return success;
        }
    }
}

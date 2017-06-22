using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Liquid.Services
{
    public class LiquidTemplateManager : ILiquidTemplateManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IEnumerable<ITemplateContextHandler> _templateContextHandlers;

        public LiquidTemplateManager(IMemoryCache memoryCache, IEnumerable<ITemplateContextHandler> templateContextHandlers)
        {
            _memoryCache = memoryCache;
            _templateContextHandlers = templateContextHandlers;
        }

        public Task RenderAsync(string source, TextWriter textWriter, TextEncoder encoder, TemplateContext context)
        {
            var errors = Enumerable.Empty<string>();

            var result = _memoryCache.GetOrCreate<FluidTemplate>(source, (ICacheEntry e) =>
            {
                if (FluidTemplate.TryParse(source, out var parsed, out errors))
                {
                    // Define a default sliding expiration to prevent the 
                    // cache from being filled and still apply some micro-caching
                    // in case the template is use commonly
                    e.SetSlidingExpiration(TimeSpan.FromSeconds(30));
                    return parsed;
                }
                else
                {
                    return null;
                }
            });

            if (result == null)
            {
                FluidTemplate.TryParse(String.Join(System.Environment.NewLine, errors), out result, out errors);
            }

            foreach (var contextHandler in _templateContextHandlers)
            {
                contextHandler.OnTemplateProcessing(context);
            }

            return result.RenderAsync(textWriter, encoder, context);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Liquid.Services
{
    public class LiquidTemplateManager : ILiquidTemplateManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        private IEnumerable<ITemplateContextHandler> _contextHandlers;

        public LiquidTemplateManager(IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
        }

        public Task RenderAsync(string template, TextWriter textWriter, TextEncoder encoder, TemplateContext context)
        {
            var errors = Enumerable.Empty<string>();
            FluidTemplate result;

            result = _memoryCache.GetOrCreate<FluidTemplate>(template, (ICacheEntry e) =>
            {
                if (FluidTemplate.TryParse(template, out result, out errors))
                {
                    // Define a default sliding expiration to prevent the 
                    // cache from being filled and still apply some micro-caching
                    // in case the template is use commonly
                    e.SetSlidingExpiration(TimeSpan.FromSeconds(30));
                    return result;
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

            // Resolve the handlers lazyly to prevent cylic dependencies
            _contextHandlers = _contextHandlers ?? _serviceProvider.GetService<IEnumerable<ITemplateContextHandler>>();

            foreach (var contextHandler in _contextHandlers)
            {
                contextHandler.OnTemplateProcessing(context);
            }

            return result.RenderAsync(textWriter, encoder, context);
        }
    }
}

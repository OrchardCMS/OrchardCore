using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Liquid.Filters
{
    public class UrlContentFilter : ITemplateContextHandler
    {
        public void OnTemplateProcessing(TemplateContext context)
        {   
            context.Filters.AddFilter("content_url", (input, arguments, ctx) =>
            {
                object urlHelper;
                if (!ctx.AmbientValues.TryGetValue("UrlHelper", out urlHelper))
                {
                    throw new ArgumentException("UrlHelper missing while invoking 'displayUrl'");
                }

                return new StringValue( ((IUrlHelper)urlHelper).Content(input.ToStringValue()));
            });
        }
    }
}

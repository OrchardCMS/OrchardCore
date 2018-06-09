using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Cache;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheExpiresOnTag : ArgumentsTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, FilterArgument[] arguments)
        {
            if (arguments.Length < 1)
            {
                // No expiry has been provided, so return
                return Completion.Normal;
            }

            if (!context.AmbientValues.TryGetValue("Services", out var servicesObj))
            {
                throw new ArgumentException("Services missing while invoking 'cache_expires_on' tag");
            }

            var services = servicesObj as IServiceProvider;

            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            if (cacheScopeManager == null)
            {
                return Completion.Normal;
            }

            var value = DateTimeOffset.MinValue;
            var input = await arguments[0].Expression.EvaluateAsync(context);

            if (input.Type == FluidValues.String)
            {
                var stringValue = input.ToStringValue();
                if (!DateTimeOffset.TryParse(stringValue, context.CultureInfo, DateTimeStyles.AssumeUniversal, out value))
                {
                    return Completion.Normal;
                }
            }
            else
            {
                switch (input.ToObjectValue())
                {
                    case DateTime dateTime:
                        value = dateTime;
                        break;

                    case DateTimeOffset dateTimeOffset:
                        value = dateTimeOffset;
                        break;

                    default:
                        return Completion.Normal;
                }
            }

            if (value != DateTimeOffset.MinValue)
            {
                cacheScopeManager.WithExpiryOn(value);
            }

            return Completion.Normal;
        }
    }
}
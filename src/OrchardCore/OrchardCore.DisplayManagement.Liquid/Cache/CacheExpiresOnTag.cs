using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;

namespace OrchardCore.DynamicCache.Liquid
{
    public class CacheExpiresOnTag
    {
        public static async ValueTask<Completion> WriteToAsync(Expression argument, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            var cacheScopeManager = services.GetService<ICacheScopeManager>();

            if (cacheScopeManager == null)
            {
                return Completion.Normal;
            }

            DateTimeOffset value;
            var input = await argument.EvaluateAsync(context);
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

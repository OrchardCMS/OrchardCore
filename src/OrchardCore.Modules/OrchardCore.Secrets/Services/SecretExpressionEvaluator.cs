using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;

namespace OrchardCore.Secrets.Services
{
    public class SecretExpressionEvaluator : ISecretExpressionEvaluator
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly SecretOptions _secretOptions;

        public SecretExpressionEvaluator(ILiquidTemplateManager liquidTemplateManager, IOptions<SecretOptions> secretOptions)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _secretOptions = secretOptions.Value;
        }

        public Task<string> EvaluateAsync(string template)
        {
            var context = _liquidTemplateManager.Context;
            context.AddAsyncFilters(_secretOptions);

            return _liquidTemplateManager.RenderAsync(template, NullEncoder.Default, null, null);
        }

        public bool IsSecretExpression(string template)
        {
            if (String.IsNullOrEmpty(template))
            {
                return false;
            }
            var trimmed = template.Trim();
            if (trimmed.StartsWith("{{") && trimmed.EndsWith("}}"))
            {
                return true;
            }

            return false;
        }
    }

    internal static class SecretExpressionEvaluatorExtensions
    {
        internal static void AddAsyncFilters(this LiquidTemplateContext context, SecretOptions options)
        {
            context.Filters.EnsureCapacity(options.FilterRegistrations.Count);

            foreach (var registration in options.FilterRegistrations)
            {
                context.Filters.AddAsyncFilter(registration.Key, (input, arguments, ctx) =>
                {
                    var filter = (ISecretLiquidFilter)context.Services.GetRequiredService(registration.Value);
                    return filter.ProcessAsync(input, arguments, ctx);
                });
            }
        }
    }
}

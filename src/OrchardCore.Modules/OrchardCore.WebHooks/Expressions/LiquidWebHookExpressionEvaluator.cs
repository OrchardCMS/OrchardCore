using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Liquid;
using OrchardCore.WebHooks.Abstractions.Models;

namespace OrchardCore.WebHooks.Expressions
{
    public class LiquidWebHookExpressionEvaluator : IWebHookExpressionEvaluator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public LiquidWebHookExpressionEvaluator(
            IServiceProvider serviceProvider,
            ILiquidTemplateManager liquidTemplateManager
        )
        {
            _serviceProvider = serviceProvider;
            _liquidTemplateManager = liquidTemplateManager;
        }

        public async Task<JObject> RenderAsync(WebHook webHook, WebHookNotificationContext context)
        {
            var templateContext = await CreateTemplateContextAsync(webHook, context);

            var result = await _liquidTemplateManager.RenderStringAsync(
                webHook.PayloadTemplate,
                NullEncoder.Default,
                new Dictionary<string, FluidValue>() { ["WebHook"] = new ObjectValue(templateContext) });

            return String.IsNullOrWhiteSpace(result) ? new JObject() : JObject.Parse(result);
        }

        private Task<TemplateContext> CreateTemplateContextAsync(WebHook webHook, WebHookNotificationContext context)
        {
            var templateContext = new TemplateContext();
            var services = _serviceProvider;

            //templateContext.MemberAccessStrategy.Register<WebHook>();
            templateContext.SetValue(nameof(WebHook), webHook);
            templateContext.SetValue("EventId", context.EventName);

            // Add webhook notification properties e.g. ContentItem
            foreach (var item in context.Properties)
            {
                if (item.Value == null) continue;
                //templateContext.MemberAccessStrategy.Register(item.Value.GetType());
                templateContext.SetValue(item.Key, item.Value);
            }

            // Add services.
            templateContext.AmbientValues.Add("Services", services);

            // Add UrlHelper, if we have an MVC Action context.
            var actionContext = services.GetService<IActionContextAccessor>()?.ActionContext;
            if (actionContext != null)
            {
                var urlHelperFactory = services.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);
                templateContext.AmbientValues.Add("UrlHelper", urlHelper);
            }

            return Task.FromResult(templateContext);
        }
    }
}

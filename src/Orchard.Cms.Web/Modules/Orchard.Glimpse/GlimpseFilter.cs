using System;
using Glimpse.Common;
using Glimpse.Initialization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Orchard.ResourceManagement;

namespace Orchard.Glimpse
{
    public class GlimpseFilter : IResultFilter
    {
        private readonly Guid _requestId;
        private readonly ResourceOptions _resourceOptions;
        private readonly IResourceManager _resourceManager;

        public GlimpseFilter(
            IGlimpseContextAccessor context, 
            IResourceOptionsProvider resourceOptionsProvider,
            IResourceManager resourceManager)
        {
            _requestId = context.RequestId;
            _resourceOptions = resourceOptionsProvider.BuildInstance();
            _resourceManager = resourceManager;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!(filterContext.Result is ViewResult))
            {
                return;
            }

            var builder = new HtmlContentBuilder();

            builder.AppendHtml(
                $@"<script src=""{_resourceOptions.HudScriptTemplate}"" id=""__glimpse_hud"" data-request-id=""{_requestId.ToString("N")}"" data-client-template=""{_resourceOptions.ClientScriptTemplate}"" data-context-template=""{_resourceOptions.ContextTemplate}"" data-metadata-template=""{_resourceOptions.MetadataTemplate}"" async></script>
                   <script src=""{_resourceOptions.BrowserAgentScriptTemplate}"" id=""__glimpse_browser_agent"" data-request-id=""{_requestId.ToString("N")}"" data-message-ingress-template=""{_resourceOptions.MessageIngressTemplate}"" async></script>"
                );

            _resourceManager.RegisterFootScript(builder);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
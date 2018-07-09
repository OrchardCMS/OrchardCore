using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using OrchardCore.Hosting.ShellBuilders;
using Serilog.Core;
using Serilog.Events;

namespace OrchardCore.Logging
{
    class TenantEnricher : ILogEventEnricher
    {
        readonly IHttpContextAccessor _httpAccessor;
        public TenantEnricher(IHttpContextAccessor httpAccessor)
        {
            _httpAccessor = httpAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = _httpAccessor.HttpContext;

            // If there is no ShellContext in the Features then the log is rendered from the Host
            var tenantName = context.Features.Get<ShellContext>()?.Settings?.Name ?? "None";
            propertyFactory.CreateProperty("TenantName", tenantName);

        }
    }
}
